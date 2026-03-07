namespace SecureFolderFS.Maui.Helpers
{
    internal static class EasingHelpers
    {
        public static readonly Easing QuarticIn = new(x => Math.Pow(x, 4));

        public static readonly Easing QuarticOut = new(x => 1 - Math.Pow(1 - x, 4));

        public static readonly Easing QuarticInOut = new(x => x < 0.5 ? 8 * Math.Pow(x, 4) : 1 - Math.Pow(-2 * x + 2, 4) / 2);

        public static readonly Easing CubicBezierOut = CubicBezier(0.25, 0.46, 0.45, 0.94);

        public static readonly Easing EaseOutExpo = CubicBezier(0.16, 1, 0.3, 1);

        private static Easing CubicBezier(double x1, double y1, double x2, double y2)
        {
            return new Easing(t =>
            {
                t = Math.Clamp(t, 0.0, 1.0);

                var cx = 3d * x1;
                var bx = 3d * (x2 - x1) - cx;
                var ax = 1d - cx - bx;

                var cy = 3d * y1;
                var by = 3d * (y2 - y1) - cy;
                var ay = 1d - cy - by;

                var x = t;
                var tEstimate = t;

                for (var i = 0; i < 8; i++)
                {
                    var xEstimate = ((ax * tEstimate + bx) * tEstimate + cx) * tEstimate;
                    var dx = xEstimate - x;
                    if (Math.Abs(dx) < 1e-6)
                        break;

                    var derivative = (3d * ax * tEstimate + 2d * bx) * tEstimate + cx;
                    if (Math.Abs(derivative) < 1e-6) break;

                    tEstimate -= dx / derivative;
                    tEstimate = Math.Clamp(tEstimate, 0.0, 1.0);
                }

                var y = ((ay * tEstimate + by) * tEstimate + cy) * tEstimate;
                return y;
            });
        }
    }
}
