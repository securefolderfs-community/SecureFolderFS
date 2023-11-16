﻿using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="StreamSerializer"/>
    public sealed class DoubleSerializedStreamSerializer : StreamSerializer
    {
        /// <summary>
        /// A single instance of <see cref="DoubleSerializedStreamSerializer"/>.
        /// </summary>
        public new static DoubleSerializedStreamSerializer Instance { get; } = new();

        /// <inheritdoc/>
        public override Task<Stream> SerializeAsync(object? data, Type dataType, CancellationToken cancellationToken = default)
        {
            if (data is IDictionary serializedDictionary)
            {
                var actualDictionary = new Dictionary<object, object?>();
                foreach (DictionaryEntry item in serializedDictionary)
                {
                    actualDictionary[item.Key] = item.Value is ISerializedModel serializedModel
                        ? serializedModel.GetValue<object?>()
                        : item.Value;
                }

                return base.SerializeAsync(actualDictionary, actualDictionary.GetType(), cancellationToken);
            }

            return base.SerializeAsync(data, dataType, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<object?> DeserializeAsync(Stream serialized, Type dataType, CancellationToken cancellationToken = default)
        {
            var deserialized = await base.DeserializeAsync(serialized, dataType, cancellationToken);
            if (deserialized is IDictionary deserializedDictionary)
            {
                var actualDictionary = new Dictionary<object, ISerializedModel?>();
                foreach (DictionaryEntry item in deserializedDictionary)
                {
                    actualDictionary[item.Key] = item.Value is JsonElement jsonElement
                        ? new JsonSerializedData(jsonElement)
                        : new NonSerializedData(item.Value);
                }

                return actualDictionary;
            }

            return deserialized;
        }

        /// <inheritdoc cref="ISerializedModel"/>
        private sealed class JsonSerializedData : ISerializedModel
        {
            private readonly JsonElement _jsonElement;
            private object? _deserialized;

            public JsonSerializedData(JsonElement jsonElement)
            {
                _jsonElement = jsonElement;
            }

            /// <inheritdoc/>
            public T? GetValue<T>()
            {
                _deserialized ??= _jsonElement.Deserialize<T?>();
                return _deserialized.TryCast<T?>();
            }
        }

        /// <inheritdoc cref="ISerializedModel"/>
        private sealed class NonSerializedData : ISerializedModel
        {
            private readonly object? _value;

            public NonSerializedData(object? value)
            {
                _value = value;
            }

            /// <inheritdoc/>
            public T? GetValue<T>()
            {
                return _value.TryCast<T>();
            }
        }
    }
}
