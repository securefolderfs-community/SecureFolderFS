using System;
using SkiaSharp;

namespace SecureFolderFS.Uno.UserControls.Introduction
{
    /// <summary>
    /// Paints the animated, grainy mesh-gradient used behind the introduction slides.
    /// The color field is drawn at a low resolution and upscaled, which gives the soft,
    /// blurred gradient look; a static noise tile on top provides the film-grain texture.
    /// </summary>
    internal sealed class IntroductionBackgroundRenderer : IDisposable
    {
        private const int FIELD_MAX_SIZE = 160; // the color field is rendered at most this large, then upscaled
        private const int NOISE_TILE_SIZE = 256;
        private const float GRAIN_SCALE = 1.6f; // enlarges the noise texels so the grain reads clearly
        private const float REVEAL_BAND = 0.22f; // relative height of the soft edge of the reveal wipe
        private const float REVEAL_LIFT = 0.30f; // relative distance the colors travel upward while revealing

        /// <summary>
        /// Domain-warped fractal noise: fbm distorted by two nested fbm passes folds the color
        /// bands into the marbled, swirling shapes that plain gradient blobs cannot produce.
        /// Runs on the tiny low-res field surface, so it is cheap even on CPU rasterizers.
        /// </summary>
        private const string FIELD_SKSL =
            """
            uniform float2 uSize;
            uniform float uTime;
            uniform float uLift;
            uniform float3 uBase;
            uniform float3 uBright;
            uniform float3 uIce;
            uniform float3 uDark;

            float hash(float2 p) {
                p = fract(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return fract(p.x * p.y);
            }

            float vnoise(float2 p) {
                float2 i = floor(p);
                float2 f = fract(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
            }

            float fbm(float2 p) {
                float v = 0.0;
                float a = 0.55;
                for (int i = 0; i < 3; i++) {
                    v += a * vnoise(p);
                    p = p * 2.03 + float2(17.0, 9.0);
                    a *= 0.45;
                }
                return v;
            }

            half4 main(float2 frag) {
                float2 uv = frag / uSize;
                float2 p = float2(uv.x * uSize.x / uSize.y, uv.y + uLift) * 1.35;

                float t = uTime * 0.15;
                float2 q = float2(
                    fbm(p + t * float2(0.9, 0.6)),
                    fbm(p + float2(5.2, 1.3) - t * float2(0.5, 0.8)));
                float2 r = float2(
                    fbm(p + 2.4 * q + float2(1.7, 9.2) + t * float2(0.6, -0.4)),
                    fbm(p + 2.4 * q + float2(8.3, 2.8) + t * float2(-0.7, 0.5)));
                float f = fbm(p + 2.6 * r);

                float3 col = mix(uBase, uBright, smoothstep(0.26, 0.76, f));
                col = mix(col, uIce, smoothstep(0.48, 0.85, q.y) * 0.9);
                col = mix(col, uDark, smoothstep(0.40, 0.78, r.x));
                col = mix(col, uDark, 0.22 * uv.y);

                return half4(half3(col), 1.0);
            }
            """;

        private static readonly SKRuntimeEffect? FieldEffect = SKRuntimeEffect.CreateShader(FIELD_SKSL, out _);

        private SKBitmap? _noiseBitmap;
        private SKShader? _grainShader;
        private SKSurface? _fieldSurface;
        private SKSizeI _fieldSize;

        private GradientBlob[] _blobs = [];
        private SKColor _baseTop;
        private SKColor _baseBottom;
        private byte _grainAlpha;
        private bool _isLight;
        private float[] _colorBase = [];
        private float[] _colorBright = [];
        private float[] _colorIce = [];
        private float[] _colorDark = [];

        /// <summary>
        /// Gets or sets whether the field upscale uses cubic resampling. Keep enabled on
        /// GPU-composited hosts; disable on CPU-rasterized hosts where cubic is expensive.
        /// </summary>
        public bool HighQualitySampling { get; set; } = true;

        /// <summary>
        /// Gets or sets the bounds (in canvas units) of the elevated content a drop shadow
        /// is drawn beneath, or null for no shadow.
        /// </summary>
        public SKRect? ShadowBounds { get; set; }

        /// <summary>
        /// Gets or sets the opacity (0..1) of the content drop shadow.
        /// </summary>
        public float ShadowOpacity { get; set; }

        /// <summary>
        /// Gets or sets the corner radius of the content drop shadow.
        /// </summary>
        public float ShadowCornerRadius { get; set; } = 8f;

        public IntroductionBackgroundRenderer()
        {
            SetTheme(light: false);
        }

        public void SetTheme(bool light)
        {
            _isLight = light;
            if (light)
            {
                _colorBase = [0.66f, 0.78f, 0.90f]; // #A9C6E6
                _colorBright = [0.37f, 0.66f, 0.94f]; // #5FA8F0
                _colorIce = [1.00f, 1.00f, 1.00f]; // #FFFFFF
                _colorDark = [0.30f, 0.43f, 0.58f]; // #4C6E93

                _baseTop = new SKColor(0xDE, 0xEB, 0xF8);
                _baseBottom = new SKColor(0xA6, 0xC4, 0xE2);
                _grainAlpha = 0x18;
                _blobs =
                [
                    new(new SKColor(0x2E, 0x8B, 0xEE, 0xDC), 0.80f, 2.0f, -18f, 4f, 0.25f, 0.32f, 0.22f, 0.18f, 0.33f, 0.21f, 0.0f),
                    new(new SKColor(0xFF, 0xFF, 0xFF, 0xE6), 0.50f, 2.3f, 24f, -6f, 0.62f, 0.12f, 0.26f, 0.16f, 0.27f, 0.36f, 1.9f),
                    new(new SKColor(0x4A, 0x6E, 0x96, 0xC8), 0.68f, 2.4f, 30f, 3f, 0.85f, 0.52f, 0.18f, 0.24f, 0.15f, 0.24f, 3.6f),
                    new(new SKColor(0x15, 0x5D, 0xAD, 0xA5), 0.72f, 1.6f, -40f, -5f, 0.30f, 0.88f, 0.22f, 0.14f, 0.21f, 0.30f, 2.6f),
                    new(new SKColor(0x58, 0x7C, 0xA4, 0xB4), 0.55f, 2.0f, 12f, 7f, 0.58f, 0.98f, 0.30f, 0.12f, 0.27f, 0.12f, 5.1f),
                    new(new SKColor(0x9E, 0xCB, 0xF7, 0xC8), 0.42f, 1.8f, 65f, -4f, 0.10f, 0.10f, 0.14f, 0.18f, 0.36f, 0.27f, 4.2f)
                ];
            }
            else
            {
                _colorBase = [0.07f, 0.23f, 0.40f]; // #123A66
                _colorBright = [0.18f, 0.61f, 1.00f]; // #2E9BFF
                _colorIce = [0.66f, 0.84f, 1.00f]; // #A9D6FF
                _colorDark = [0.004f, 0.02f, 0.047f]; // #01050C

                _baseTop = new SKColor(0x04, 0x10, 0x1F);
                _baseBottom = new SKColor(0x0A, 0x21, 0x38);
                _grainAlpha = 0x26;
                _blobs =
                [
                    new(new SKColor(0x21, 0x96, 0xFF, 0xF0), 0.80f, 2.0f, -18f, 4f, 0.25f, 0.32f, 0.22f, 0.18f, 0.33f, 0.21f, 0.0f),
                    new(new SKColor(0xA8, 0xD4, 0xFF, 0x96), 0.50f, 2.3f, 24f, -6f, 0.62f, 0.12f, 0.26f, 0.16f, 0.27f, 0.36f, 1.9f),
                    new(new SKColor(0x00, 0x01, 0x04, 0xFA), 0.68f, 2.4f, 30f, 3f, 0.85f, 0.52f, 0.18f, 0.24f, 0.15f, 0.24f, 3.6f),
                    new(new SKColor(0x0D, 0x5C, 0xC0, 0xE0), 0.72f, 1.6f, -40f, -5f, 0.30f, 0.88f, 0.22f, 0.14f, 0.21f, 0.30f, 2.6f),
                    new(new SKColor(0x01, 0x04, 0x09, 0xDC), 0.55f, 2.0f, 12f, 7f, 0.58f, 0.98f, 0.30f, 0.12f, 0.27f, 0.12f, 5.1f),
                    new(new SKColor(0x4F, 0xAC, 0xFF, 0x78), 0.42f, 1.8f, 65f, -4f, 0.10f, 0.10f, 0.14f, 0.18f, 0.36f, 0.27f, 4.2f)
                ];
            }
        }

        /// <summary>
        /// Draws one frame onto <paramref name="canvas"/>. Nothing is drawn while
        /// <paramref name="reveal"/> is 0; a partially revealed frame keeps everything
        /// above the wipe front fully transparent.
        /// </summary>
        public void Render(SKCanvas canvas, float width, float height, float time, float reveal)
        {
            if (width <= 0f || height <= 0f || reveal <= 0f)
                return;

            var needsMask = reveal < 1f;
            if (needsMask)
                canvas.SaveLayer();

            DrawColorField(canvas, width, height, time, reveal);
            DrawGrain(canvas, width, height);
            DrawShadow(canvas);

            if (needsMask)
            {
                // Erase everything above the reveal front, with a soft gradient edge
                var band = height * REVEAL_BAND;
                var frontTop = height - reveal * (height + band);

                using var maskPaint = new SKPaint();
                maskPaint.BlendMode = SKBlendMode.DstIn;
                maskPaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0f, frontTop),
                    new SKPoint(0f, frontTop + band),
                    [SKColors.Transparent, SKColors.White],
                    null,
                    SKShaderTileMode.Clamp);

                canvas.DrawRect(new SKRect(0, 0, width, height), maskPaint);
                canvas.Restore();
            }
        }

        private void DrawColorField(SKCanvas canvas, float width, float height, float time, float reveal)
        {
            var scale = Math.Min(1f, FIELD_MAX_SIZE / Math.Max(width, height));
            var fieldSize = new SKSizeI(
                Math.Max(16, (int)(width * scale)),
                Math.Max(16, (int)(height * scale)));

            if (_fieldSurface is null || fieldSize != _fieldSize)
            {
                _fieldSurface?.Dispose();
                _fieldSurface = SKSurface.Create(new SKImageInfo(fieldSize.Width, fieldSize.Height));
                _fieldSize = fieldSize;
            }

            var field = _fieldSurface.Canvas;
            var fieldWidth = (float)fieldSize.Width;
            var fieldHeight = (float)fieldSize.Height;
            var lift = (1f - reveal) * REVEAL_LIFT;

            if (FieldEffect is not null)
            {
                var uniforms = new SKRuntimeEffectUniforms(FieldEffect)
                {
                    ["uSize"] = new[] { fieldWidth, fieldHeight },
                    ["uTime"] = time,
                    ["uLift"] = lift,
                    ["uBase"] = _colorBase,
                    ["uBright"] = _colorBright,
                    ["uIce"] = _colorIce,
                    ["uDark"] = _colorDark
                };

                using var fieldShader = FieldEffect.ToShader(uniforms);
                using var fieldPaint = new SKPaint();
                fieldPaint.Shader = fieldShader;
                field.DrawRect(new SKRect(0, 0, fieldWidth, fieldHeight), fieldPaint);
            }
            else
                DrawColorFieldFallback(field, fieldWidth, fieldHeight, time, lift);

            using var snapshot = _fieldSurface.Snapshot();
            var sampling = HighQualitySampling
                ? new SKSamplingOptions(SKCubicResampler.Mitchell)
                : new SKSamplingOptions(SKFilterMode.Linear);

            canvas.DrawImage(snapshot, new SKRect(0, 0, width, height), sampling);
        }

        /// <summary>
        /// Gradient-blob approximation of the color field, used if the SkSL runtime effect
        /// is unavailable on the current Skia backend.
        /// </summary>
        private void DrawColorFieldFallback(SKCanvas field, float fieldWidth, float fieldHeight, float time, float lift)
        {
            var maxDimension = Math.Max(fieldWidth, fieldHeight);

            // Base vertical gradient
            using (var basePaint = new SKPaint())
            {
                basePaint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0f, 0f),
                    new SKPoint(0f, fieldHeight),
                    [_baseTop, _baseBottom],
                    null,
                    SKShaderTileMode.Clamp);

                field.DrawRect(new SKRect(0, 0, fieldWidth, fieldHeight), basePaint);
            }

            // Elongated, slowly rotating gradient patches drifting along Lissajous paths
            // form the sweeping bands of color. While revealing, they sit lower and rise
            // into place with the wipe.
            foreach (var blob in _blobs)
            {
                var centerX = (blob.AnchorX + blob.AmplitudeX * MathF.Sin(blob.SpeedX * time + blob.Phase)) * fieldWidth;
                var centerY = (blob.AnchorY + blob.AmplitudeY * MathF.Cos(blob.SpeedY * time + blob.Phase * 1.7f) + lift) * fieldHeight;
                var radius = blob.Radius * maxDimension;

                using var blobPaint = new SKPaint();
                blobPaint.Shader = SKShader.CreateRadialGradient(
                    default,
                    radius,
                    [blob.Color, blob.Color.WithAlpha(0)],
                    null,
                    SKShaderTileMode.Clamp);

                field.Save();
                field.Translate(centerX, centerY);
                field.RotateDegrees(blob.AngleDegrees + blob.RotationSpeed * time);
                field.Scale(blob.Stretch, 1f);
                field.DrawCircle(0f, 0f, radius, blobPaint);
                field.Restore();
            }
        }

        private void DrawShadow(SKCanvas canvas)
        {
            if (ShadowBounds is not { } bounds || ShadowOpacity <= 0f)
                return;

            var baseAlpha = _isLight ? 0x55 : 0x8C;
            using var shadowPaint = new SKPaint();
            shadowPaint.IsAntialias = true;
            shadowPaint.Color = new SKColor(0, 0, 0, (byte)(baseAlpha * Math.Clamp(ShadowOpacity, 0f, 1f)));
            shadowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20f);

            var rect = bounds;
            rect.Offset(0f, 10f);
            rect.Inflate(2f, 2f);
            canvas.DrawRoundRect(rect, ShadowCornerRadius, ShadowCornerRadius, shadowPaint);
        }

        private void DrawGrain(SKCanvas canvas, float width, float height)
        {
            // The grain is intentionally static: a fixed noise layer overlaid
            // on top of the moving colors, like film grain on footage
            if (_grainShader is null)
            {
                _noiseBitmap = CreateNoiseBitmap();
                _grainShader = _noiseBitmap.ToShader(
                    SKShaderTileMode.Repeat,
                    SKShaderTileMode.Repeat,
                    SKMatrix.CreateScale(GRAIN_SCALE, GRAIN_SCALE));
            }

            using var grainPaint = new SKPaint();
            grainPaint.BlendMode = SKBlendMode.Overlay;
            grainPaint.Color = SKColors.White.WithAlpha(_grainAlpha);
            grainPaint.Shader = _grainShader;

            canvas.DrawRect(new SKRect(0, 0, width, height), grainPaint);
        }

        private static unsafe SKBitmap CreateNoiseBitmap()
        {
            var bitmap = new SKBitmap(new SKImageInfo(NOISE_TILE_SIZE, NOISE_TILE_SIZE, SKColorType.Gray8, SKAlphaType.Opaque));
            var pixels = new Span<byte>((void*)bitmap.GetPixels(), bitmap.ByteCount);
            Random.Shared.NextBytes(pixels);

            return bitmap;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _fieldSurface?.Dispose();
            _fieldSurface = null;
            _grainShader?.Dispose();
            _grainShader = null;
            _noiseBitmap?.Dispose();
            _noiseBitmap = null;
        }

        /// <summary>
        /// A single soft gradient patch of the color field, stretched into an ellipse and
        /// slowly rotating. Coordinates, amplitudes, and the radius are relative to the field
        /// size; speeds are in radians per second, rotation in degrees per second.
        /// </summary>
        private readonly record struct GradientBlob(
            SKColor Color,
            float Radius,
            float Stretch,
            float AngleDegrees,
            float RotationSpeed,
            float AnchorX,
            float AnchorY,
            float AmplitudeX,
            float AmplitudeY,
            float SpeedX,
            float SpeedY,
            float Phase);
    }
}
