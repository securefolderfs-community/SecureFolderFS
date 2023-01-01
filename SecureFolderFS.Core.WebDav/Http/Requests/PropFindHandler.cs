using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Core.WebDav.Storage.StorageProperties;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SecureFolderFS.Core.WebDav.Constants;

namespace SecureFolderFS.Core.WebDav.Http.Requests
{
    /// <inheritdoc cref="IRequestHandler"/>
    internal sealed class PropFindHandler : IRequestHandler
    {
        /// <inheritdoc/>
        public async Task ProcessRequestAsync(IHttpContext context, IStorageService storageService, CancellationToken cancellationToken = default)
        {
            if (context.Request.Url is null)
            {
                context.Response.SetStatus(HttpStatusCode.NotFound);
                return;
            }

            var entries = new List<PropertyEntry>();
            var propertyList = new List<XName>();
            var propertyMode = await GetRequestedPropertiesAsync(context.Request, propertyList, cancellationToken);

            var storable = await storageService.GetStorableFromPathAsync(context.Request.Url.GetUriPath(), cancellationToken);
            if (storable is IFolder folder)
            {
                var depth = 0;
                if (folder is IDavFolder davFolder)
                {
                    depth = context.Request.GetEnumerationDepth();
                    if (depth > 1)
                    {
                        switch (davFolder.DepthMode)
                        {
                            case EnumerationDepthMode.Unsupported:
                                context.Response.SetStatus(HttpStatusCode.Forbidden, "Infinite depth enumeration is not supported.");
                                return;

                            case EnumerationDepthMode.Shallow:
                                depth = 0;
                                break;

                            case EnumerationDepthMode.BreadthFirst:
                                depth = 1;
                                break;

                            default:
                            case EnumerationDepthMode.Infinite:
                                _ = depth;
                                break;
                        }
                    }
                }

                await AddEntriesAsync(folder, depth, context, context.Request.Url, entries, cancellationToken);
            }
            else if (storable is IDavFile file)
            {
                entries.Add(new(context.Request.Url, file));
            }
            else
            {
                context.Response.SetStatus(HttpStatusCode.InternalServerError); // TODO: Set status based on IResult
                return;
            }

            var xMultiStatus = new XElement((XNamespace)Constants.WebDavNamespaces.DAV_NAMESPACE + "multistatus");
            var xDocument = new XDocument(xMultiStatus);

            foreach (var entry in entries)
            {
                var xResponse = new XElement((XNamespace)Constants.WebDavNamespaces.DAV_NAMESPACE + "response", new XElement((XNamespace)Constants.WebDavNamespaces.DAV_NAMESPACE + "href", entry.Uri.EncodeUriString()));
                var xPropStatValues = new XElement((XNamespace)Constants.WebDavNamespaces.DAV_NAMESPACE + "propstat");

                if (entry.Storable is IStorableExtended { Properties: IDavProperties davProperties })
                {
                    if (propertyMode == DavPropertyMode.PropertyNames)
                    {
                        await foreach (var item in davProperties.GetIdentifiersAsync(cancellationToken))
                        {
                            xPropStatValues.Add(new XElement(item.Name));
                        }

                        xResponse.Add(xPropStatValues);
                    }
                    else
                    {
                        var properties = new List<XName>();
                        if (propertyMode.HasFlag(DavPropertyMode.AllProperties))
                        {
                            await foreach (var item in davProperties.GetIdentifiersAsync(cancellationToken))
                            {
                                if (!item.IsExpensive)
                                    await AddPropertyAsync(xResponse, xPropStatValues, entry.Storable, (XName)item.Name, properties, cancellationToken);
                            }
                        }

                        if (propertyMode.HasFlag(DavPropertyMode.SelectedProperties))
                        {
                            foreach (var item in propertyList)
                            {
                                await AddPropertyAsync(xResponse, xPropStatValues, entry.Storable, item, properties, cancellationToken);
                            }
                        }

                        if (xPropStatValues.HasElements)
                            xResponse.Add(xPropStatValues);
                    }
                }

                xPropStatValues.Add(new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "status", "HTTP/1.1 200 OK"));
                xMultiStatus.Add(xResponse);
            }

            await context.Response.SendResponseAsync(HttpStatusCode.MultiStatus, xDocument, cancellationToken);
        }

        private static async Task AddPropertyAsync(XContainer xResponse, XContainer xPropStatValues, IStorable storable, XName propertyName, ICollection<XName> addedProperties, CancellationToken cancellationToken)
        {
            if (!addedProperties.Contains(propertyName))
            {
                if (storable is not IStorableExtended { Properties: IDavProperties davProperties })
                    return;

                addedProperties.Add(propertyName);
                try
                {
                    await foreach (var item in davProperties.GetIdentifiersAsync(cancellationToken))
                    {
                        if (item.Name != propertyName)
                            continue;

                        var value = await davProperties.GetPropertyAsync(propertyName.ToString(), cancellationToken);
                        var xProp = xPropStatValues.Element((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "prop");
                        if (xProp is null)
                        {
                            xProp = new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "prop");
                            xPropStatValues.Add(xProp);
                        }

                        xProp.Add(new XElement(propertyName, value));
                        return;
                    }

                    xResponse.Add(new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "propstat",
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "prop", new XElement(propertyName, null)),
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "status", "HTTP/1.1 404 Not Found"),
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "responsedescription", $"Property {propertyName} is not supported.")));
                }
                catch (Exception)
                {
                    xResponse.Add(new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "propstat",
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "prop", new XElement(propertyName, null)),
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "status", "HTTP/1.1 500 Internal server error"),
                        new XElement((XNamespace)WebDavNamespaces.DAV_NAMESPACE + "responsedescription", $"Property {propertyName} on item {storable.Name} raised an exception.")));
                }
            }
        }

        private static async Task<DavPropertyMode> GetRequestedPropertiesAsync(IHttpRequest request, ICollection<XName> properties, CancellationToken cancellationToken)
        {
            // Create an XML document from the stream
            var xDocument = await request.GetDocumentFromRequestAsync(cancellationToken);
            if (xDocument?.Root == null || xDocument.Root.Name != (XNamespace)WebDavNamespaces.DAV_NAMESPACE + "propfind")
                return DavPropertyMode.AllProperties;

            // Obtain the propfind node
            var xPropFind = xDocument.Root;

            // If there is no child-node, then return all properties
            var xProps = xPropFind.Elements();
            if (!xProps.Any())
                return DavPropertyMode.AllProperties;

            // Add all entries to the list
            var propertyMode = DavPropertyMode.None;
            foreach (var xProp in xPropFind.Elements())
            {
                // Check if we should fetch all property names
                if (xProp.Name == (XNamespace)WebDavNamespaces.DAV_NAMESPACE + "propname")
                {
                    propertyMode = DavPropertyMode.PropertyNames;
                }
                else if (xProp.Name == (XNamespace)WebDavNamespaces.DAV_NAMESPACE + "allprop")
                {
                    propertyMode = DavPropertyMode.AllProperties;
                }
                else if (xProp.Name == (XNamespace)WebDavNamespaces.DAV_NAMESPACE + "include")
                {
                    // Include properties
                    propertyMode = DavPropertyMode.AllProperties | DavPropertyMode.SelectedProperties;

                    // Include all specified properties
                    foreach (var xSubProp in xProp.Elements())
                        properties.Add(xSubProp.Name);
                }
                else
                {
                    propertyMode = DavPropertyMode.SelectedProperties;

                    // Include all specified properties
                    foreach (var xSubProp in xProp.Elements())
                        properties.Add(xSubProp.Name);
                }
            }

            return propertyMode;
        }

        private static async Task AddEntriesAsync(IFolder folder, int depth, IHttpContext httpContext, Uri uri, ICollection<PropertyEntry> entries, CancellationToken cancellationToken)
        {
            // Add the collection to the list
            entries.Add(new(uri, folder));

            // If we have enough depth, then add the children
            if (depth > 0)
            {
                await foreach (var item in folder.GetItemsAsync(StorableKind.All, cancellationToken))
                {
                    var subUri = uri.CombinePath(item.Name);
                    if (item is IFolder subFolder)
                    {
                        await AddEntriesAsync(subFolder, depth - 1, httpContext, subUri, entries, cancellationToken);
                    }
                    else
                    {
                        entries.Add(new(subUri, item));
                    }
                }
            }
        }

        private readonly record struct PropertyEntry(Uri Uri, IStorable Storable);
    }
}
