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
        private const float REVEAL_LIFT = 0.25f; // relative distance the colors travel upward while revealing

        private SKBitmap? _noiseBitmap;
        private SKShader? _grainShader;
        private SKSurface? _fieldSurface;
        private SKSizeI _fieldSize;

        private GradientBlob[] _blobs = [];
        private SKColor _baseTop;
        private SKColor _baseBottom;
        private byte _grainAlpha;

        /// <summary>
        /// Gets or sets whether the field upscale uses cubic resampling. Keep enabled on
        /// GPU-composited hosts; disable on CPU-rasterized hosts where cubic is expensive.
        /// </summary>
        public bool HighQualitySampling { get; set; } = true;

        public IntroductionBackgroundRenderer()
        {
            SetTheme(light: false);
        }

        public void SetTheme(bool light)
        {
            if (light)
            {
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
            var lift = (1f - reveal) * REVEAL_LIFT;
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

            using var snapshot = _fieldSurface.Snapshot();
            var sampling = HighQualitySampling
                ? new SKSamplingOptions(SKCubicResampler.Mitchell)
                : new SKSamplingOptions(SKFilterMode.Linear);

            canvas.DrawImage(snapshot, new SKRect(0, 0, width, height), sampling);
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
