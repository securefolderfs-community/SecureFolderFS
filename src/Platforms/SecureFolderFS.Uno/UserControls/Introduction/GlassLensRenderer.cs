using System;
using SkiaSharp;

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    /// <summary>
    /// Paints a glass-style lens over a scene snapshot. A thick convex glass disc
    /// with true per-pixel refraction. The interior magnifies the content while samples
    /// sweep outward across the rim, visibly bending the surrounding image around the
    /// edge, with chromatic dispersion fringing the bent band like real glass.
    /// </summary>
    internal sealed class GlassLensRenderer : IDisposable
    {
        private const float SOURCE_PADDING = 1.6f; // sampled region extent, relative to the radius
        private const float SOURCE_BLUR_SIGMA = 2.5f; // frost of the wrap band and the seam - the stretch stays sharp
        private const float STRETCH_START = 0.70f; // where the interior content starts stretching outward
        private const float WRAP_START = 0.84f; // where the outer wrap band takes over
        private const float STRETCH_GAIN = 0.5f; // how little source the stretch band covers (smaller = more stretch)
        private const float FROST_START = 0.88f; // frost onset of the wrap band
        private const float EDGE_OVERSHOOT = 0.42f; // how far past the rim the wrap band's samples reach
        private const float EDGE_DISPERSION = 0.028f; // chromatic separation in the bent bands (relative radius)
        private const float INNER_BEND = 0.03f; // near-imperceptible curvature of the interior glyphs
        private const float INNER_DISPERSION = 0.005f; // subtle chromatic fringing across the interior
        private const float RIM_CHROMATIC_OFFSET = 1.6f; // radial separation of the specular ring's color channels

        /// <summary>
        /// Refraction of a thick glass disc with a flat center. Coordinates are in canvas
        /// units. Three radial zones: a flat interior; a stretch band where magnification
        /// rises sharply so the inner content smears outward toward the rim; and a wrap band
        /// where samples sweep past the rim, compressing the surroundings around the edge.
        /// The opposing slopes meet at uWrapStart. That seam is masked with a localized
        /// frost bump and a slight dim, like light scattering inside the glass fold.
        /// </summary>
        private const string LENS_SKSL =
            """
            uniform shader uContent;
            uniform shader uFrosted;
            uniform float2 uCenter;
            uniform float uRadius;
            uniform float uZoom;
            uniform float uStretchStart;
            uniform float uWrapStart;
            uniform float uStretchGain;
            uniform float uFrostStart;
            uniform float uOvershoot;
            uniform float uEdgeDispersion;
            uniform float uInnerBend;
            uniform float uInnerDispersion;

            half4 main(float2 frag) {
                float2 offset = frag - uCenter;
                float r = length(offset);
                float rn = min(r / uRadius, 1.0);
                float2 dir = r > 0.001 ? offset / r : float2(0.0, 0.0);

                // Flat interior mapping
                float interiorRn = (rn / uZoom) * (1.0 + uInnerBend * rn * rn);

                // Stretch band. The sample radius saturates (ease-out), so magnification keeps
                // rising toward the band's end and the content visibly smears outward
                float bandWidth = uWrapStart - uStretchStart;
                float t = clamp((rn - uStretchStart) / bandWidth, 0.0, 1.0);
                float ease = 1.0 - pow(1.0 - t, 2.2);
                float s0 = (uStretchStart / uZoom) * (1.0 + uInnerBend * uStretchStart * uStretchStart);
                float sEnd = s0 + bandWidth / uZoom * uStretchGain;
                float stretchRn = s0 + (sEnd - s0) * ease;
                float innerRn = rn < uStretchStart ? interiorRn : stretchRn;

                // Wrap band. Samples sweep from the stretch band's end past the rim,
                // compressing the remaining inner content and the surroundings around the edge
                float w = clamp((rn - uWrapStart) / (1.0 - uWrapStart), 0.0, 1.0);
                float wrap = pow(w, 1.6);
                float wrapRn = sEnd + (1.0 + uOvershoot - sEnd) * wrap;

                // Soft handoff between the opposing zones
                float blend = smoothstep(uWrapStart - 0.025, uWrapStart + 0.025, rn);
                float baseRn = mix(innerRn, wrapRn, blend);

                float dispersion = uInnerDispersion * rn + uEdgeDispersion * (0.15 * ease + wrap);
                float3 sampleRn = baseRn + float3(-dispersion, 0.0, dispersion);
                float3 sampleR = sampleRn * uRadius;

                float2 pr = uCenter + dir * sampleR.x;
                float2 pg = uCenter + dir * sampleR.y;
                float2 pb = uCenter + dir * sampleR.z;

                half3 sharp = half3(uContent.eval(pr).r, uContent.eval(pg).g, uContent.eval(pb).b);
                half3 frosted = half3(uFrosted.eval(pr).r, uFrosted.eval(pg).g, uFrosted.eval(pb).b);

                // The seam between the zones is masked by a localized frost bump and a slight dim
                float seamDelta = (rn - uWrapStart) / 0.035;
                float seam = exp(-seamDelta * seamDelta);
                float frost = max(smoothstep(uFrostStart, 1.0, rn), 0.8 * seam);

                half3 col = mix(sharp, frosted, half(frost));
                col *= 1.0 + 0.10 * half(wrap);
                col *= 1.0 - 0.12 * half(seam);
                return half4(min(col, half3(1.0)), 1.0);
            }
            """;

        private static readonly SKRuntimeEffect? LensEffect = SKRuntimeEffect.CreateShader(LENS_SKSL, out _);

        private SKSurface? _sourceSurface;
        private SKSurface? _frostSurface;
        private int _sourceSize;
        private readonly SKPaint _sourceBlurPaint;
        private readonly SKPaint _dropShadowPaint;
        private readonly SKPaint _innerEdgePaint;
        private readonly SKPaint _rimSpecularPaint;

        public GlassLensRenderer()
        {
            // The under-glass frosting
            _sourceBlurPaint = new SKPaint
            {
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(SOURCE_BLUR_SIGMA, SOURCE_BLUR_SIGMA)
            };

            _dropShadowPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(0, 0, 0, 40),
                StrokeWidth = 4f,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 7f)
            };

            // Thin dark contour just inside the edge, selling the glass thickness
            _innerEdgePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = new SKColor(0, 0, 0, 36),
                StrokeWidth = 1.6f,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 1.2f)
            };

            _rimSpecularPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.2f,
                IsAntialias = true,
                BlendMode = SKBlendMode.Plus
            };
        }

        /// <summary>
        /// Draws the lens onto <paramref name="canvas"/>.
        /// </summary>
        /// <param name="canvas">The target canvas.</param>
        /// <param name="scene">A snapshot of the fully composed scene beneath the lens.</param>
        /// <param name="center">The lens center, in canvas units.</param>
        /// <param name="radius">The undeformed lens radius.</param>
        /// <param name="radiusX">The horizontal radius after squash-and-stretch deformation.</param>
        /// <param name="radiusY">The vertical radius after squash-and-stretch deformation.</param>
        /// <param name="zoom">The magnification of the lens interior.</param>
        public void Draw(SKCanvas canvas, SKImage scene, SKPoint center, float radius, float radiusX, float radiusY, float zoom)
        {
            using var lensSource = CreateLensSource(scene, center, radius, frosted: false);
            using var frostedSource = CreateLensSource(scene, center, radius, frosted: true);

            // The squash deformation is a plain scale about the center.
            // The refracted content deforms together with the glass, like a liquid surface would
            canvas.Save();
            canvas.Translate(center.X, center.Y);
            canvas.Scale(radiusX / radius, radiusY / radius);
            canvas.Translate(-center.X, -center.Y);

            // Soft drop shadow around the disc
            canvas.DrawCircle(center, radius + 1.5f, _dropShadowPaint);

            // Refracted interior
            canvas.Save();
            using (var clipPath = new SKPath())
            {
                clipPath.AddCircle(center.X, center.Y, radius);
                canvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
            }

            if (LensEffect is not null)
            {
                var uniforms = new SKRuntimeEffectUniforms(LensEffect)
                {
                    ["uCenter"] = new[] { center.X, center.Y },
                    ["uRadius"] = radius,
                    ["uZoom"] = zoom,
                    ["uStretchStart"] = STRETCH_START,
                    ["uWrapStart"] = WRAP_START,
                    ["uStretchGain"] = STRETCH_GAIN,
                    ["uFrostStart"] = FROST_START,
                    ["uOvershoot"] = EDGE_OVERSHOOT,
                    ["uEdgeDispersion"] = EDGE_DISPERSION,
                    ["uInnerBend"] = INNER_BEND,
                    ["uInnerDispersion"] = INNER_DISPERSION
                };
                var children = new SKRuntimeEffectChildren(LensEffect)
                {
                    ["uContent"] = lensSource,
                    ["uFrosted"] = frostedSource
                };

                using var refractionShader = LensEffect.ToShader(uniforms, children);
                using var refractionPaint = new SKPaint();
                refractionPaint.Shader = refractionShader;

                canvas.DrawRect(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), refractionPaint);
            }
            else
            {
                // Runtime effects unavailable (fall back to a plain magnification)
                using var fallbackPaint = new SKPaint();
                fallbackPaint.Shader = lensSource;

                canvas.Save();
                canvas.Translate(center.X, center.Y);
                canvas.Scale(zoom, zoom);
                canvas.Translate(-center.X, -center.Y);
                canvas.DrawRect(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), fallbackPaint);
                canvas.Restore();
            }

            // Thin dark contour just inside the rim
            canvas.DrawCircle(center, radius - 1.5f, _innerEdgePaint);
            canvas.Restore(); // end interior clip

            // Specular edge, split spectrally. The ring is drawn once per color channel at
            // slightly offset radii with additive blending. Where the channels overlap the
            // highlight is white; at the band's borders they separate into the RGB fringes
            // seen along Liquid Glass edges on iOS
            ReadOnlySpan<SKColor> channels = [new(255, 0, 0), new(0, 255, 0), new SKColor(0, 0, 255)];
            for (var i = 0; i < channels.Length; i++)
            {
                _rimSpecularPaint.Shader = CreateRimSweep(center, channels[i]);
                canvas.DrawCircle(center, radius - 0.5f + (1 - i) * RIM_CHROMATIC_OFFSET, _rimSpecularPaint);
            }

            canvas.Restore(); // end squash transform
        }

        /// <summary>
        /// Builds the angular specular profile of the rim for one color channel with brightest
        /// toward the light (top-left), and a weaker counter-glint at the bottom-right.
        /// </summary>
        private static SKShader CreateRimSweep(SKPoint center, SKColor channel)
        {
            return SKShader.CreateSweepGradient(
                center,
                [
                    channel.WithAlpha(0x3C), // 0deg (+x)
                    channel.WithAlpha(0x96), // 45deg - counter glint
                    channel.WithAlpha(0x2E),
                    channel.WithAlpha(0x28), // 180deg (-x)
                    channel.WithAlpha(0xE6), // 225deg - main highlight (top-left)
                    channel.WithAlpha(0x2E),
                    channel.WithAlpha(0x3C) // 360deg, matches the start
                ],
                [0f, 0.125f, 0.30f, 0.50f, 0.625f, 0.78f, 1f]);
        }

        /// <summary>
        /// Extracts the region of the scene the lens samples from, optionally frosted. Keeping
        /// the blur axis-aligned to the scene (no offsets, no per-copy shifts) is what keeps the
        /// refracted content perfectly registered with the unrefracted surroundings.
        /// </summary>
        private SKShader CreateLensSource(SKImage scene, SKPoint center, float radius, bool frosted)
        {
            var padding = radius * SOURCE_PADDING;
            var size = (int)MathF.Ceiling(padding * 2f);
            if (_sourceSurface is null || _frostSurface is null || size != _sourceSize)
            {
                _sourceSurface?.Dispose();
                _frostSurface?.Dispose();
                _sourceSurface = SKSurface.Create(new SKImageInfo(size, size));
                _frostSurface = SKSurface.Create(new SKImageInfo(size, size));
                _sourceSize = size;
            }

            var surface = (frosted ? _frostSurface : _sourceSurface)!;
            var source = surface.Canvas;
            source.Clear(SKColors.Transparent);
            source.DrawImage(scene, -(center.X - padding), -(center.Y - padding), frosted ? _sourceBlurPaint : null);

            using var snapshot = surface.Snapshot();
            return snapshot.ToShader(
                SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp,
                frosted ? new SKSamplingOptions(SKFilterMode.Linear) : new SKSamplingOptions(SKCubicResampler.Mitchell),
                SKMatrix.CreateTranslation(center.X - padding, center.Y - padding));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _sourceSurface?.Dispose();
            _sourceSurface = null;
            _frostSurface?.Dispose();
            _frostSurface = null;
            _sourceBlurPaint.Dispose();
            _dropShadowPaint.Dispose();
            _innerEdgePaint.Dispose();
            _rimSpecularPaint.Dispose();
        }
    }
}
