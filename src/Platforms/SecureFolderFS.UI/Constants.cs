namespace SecureFolderFS.UI
{
    public static class Constants
    {
        public const string MAIN_WINDOW_ID = "SecureFolderFS_mainwindow";
        public const string DATA_CONTAINER_ID = "SecureFolderFS_datacontainer";
        public const string STORABLE_BOOKMARK_RID = "bookmark:";

        public static class GitHub
        {
            public const string REPOSITORY_NAME = "SecureFolderFS";
            public const string REPOSITORY_OWNER = "securefolderfs-community";
        }

        public static class FileNames
        {
            public const string KEY_FILE_EXTENSION = ".key";
            public const string VAULTS_WIDGETS_FOLDERNAME = "vaults_widgets";
            public const string SETTINGS_FOLDER_NAME = "settings";
            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
            public const string SAVED_VAULTS_FILENAME = "saved_vaults.json";
            public const string USER_SETTINGS_FILENAME = "user_settings.json";
            public const string ICON_ASSET_PATH = "Assets/AppAssets/app_icon.ico";
            public const string VAULT_SHORTCUT_FILE_EXTENSION = ".sfvault";
        }

        public static class AppThemes
        {
            public const string LIGHT_THEME = "LIGHT";
            public const string DARK_THEME = "DARK";
            public const string THEME_PREFERENCE_SETTING = "ImmersiveTheme";
        }

        public static class Application
        {
            public const string EXCEPTION_BLOCK_DATE_FORMAT = "dd.MM.yyyy HH_mm_ss";
            public const string EXCEPTION_LOG_FILENAME = "securefolderfs_exceptionlog.log";
            public const string SESSION_EXCEPTION_FILENAME = "securefolderfs_sessionexception.log";
            public const string DEFAULT_CULTURE_STRING = "en-US";
        }

        public static class Browser
        {
            public const int THUMBNAIL_MAX_PARALLELISATION = 4;
            public const int IMAGE_THUMBNAIL_MAX_SIZE = 300;
            public const int IMAGE_THUMBNAIL_QUALITY = 80;
            public const int VIDEO_THUMBNAIL_QUALITY = 80;
        }

        public static class FileData
        {
            public const string DESKTOP_INI_ICON_CONFIGURATION = """
                                                                 [.ShellClassInfo]
                                                                 IconResource={0},0
                                                                 InfoTip={1}
                                                                 [ViewState]
                                                                 FolderType = Generic
                                                                 """;
        }
        
        public static class Introduction
        {
            public const string BACKGROUND_WEBVIEW = """
                                                                  <!doctype html>
                                                                  <html>
                                                                  <head>
                                                                  <meta charset="utf-8"/>
                                                                  <meta name="viewport" content="width=device-width,initial-scale=1"/>
                                                                  <title>Fragment Shader</title>
                                                                  <style>
                                                                  body, html, canvas {
                                                                    background: transparent !important;
                                                                    margin: 0;
                                                                    padding: 0;
                                                                    width: 100%;
                                                                    height: 100%;
                                                                    overflow: hidden;
                                                                  }
                                                                  </style>
                                                                  </head>
                                                                  <body>
                                                                  <canvas id="gl"></canvas>
                                                                  <script>
                                                                  const vs = `#version 300 es
                                                                  in vec2 aPos;
                                                                  out vec2 vUv;
                                                                  void main() {
                                                                    vUv = (aPos + 1.0) * 0.5;
                                                                    gl_Position = vec4(aPos, 0.0, 1.0);
                                                                  }
                                                                  `;
                                                                  
                                                                  const fs = `#version 300 es
                                                                  precision highp float;
                                                                  in vec2 vUv;
                                                                  out vec4 fragColor;
                                                                  uniform vec2 iResolution;
                                                                  uniform float iTime;
                                                                  
                                                                  float sstep(float e0, float e1, float x) {
                                                                    return smoothstep(e0, e1, x);
                                                                  }
                                                                  
                                                                  void main() {
                                                                    vec2 uv = vUv;
                                                                    uv.y = (uv.y - 0.5) * 2.0;

                                                                    float t = iTime;

                                                                    // Color Palette (replaced dynamically)
                                                                    vec3 bgColor   = c_bg;
                                                                    vec3 waveColor = c_wave;
                                                                    vec3 c = bgColor;

                                                                    for (int i = 1; i < 8; i++) {
                                                                        float fi = float(i);
                                                                        float x = uv.x * 1.6 + t * sin(fi * 3.3) * 0.4 + fi * 4.3;
                                                                        float f = 0.3 * cos(fi) * sin(x);
                                                                        float b = sin(uv.x * sin(fi + t * 0.2) * 3.4 + cos(fi + 7.1)) * 0.1 + 0.896;
                                                                        float y = smoothstep(b, 1.0, 1.0 - abs(uv.y - f)) * (b - 0.8) * 2.5;

                                                                        float strength = 0.35 + fi * 0.06;
                                                                        c = mix(c, waveColor, y * strength);
                                                                    }

                                                                    fragColor = vec4(c, 1.0);
                                                                  }
                                                                  `;
                                                                  
                                                                  const canvas = document.getElementById('gl');
                                                                  const gl = canvas.getContext('webgl2', {antialias: true});
                                                                  if (!gl) {
                                                                    document.body.innerHTML = "WebGL2 not supported";
                                                                  } else {
                                                                    // resize
                                                                    function resize() {
                                                                      const dpr = window.devicePixelRatio || 1;
                                                                      canvas.width = Math.floor(innerWidth * dpr);
                                                                      canvas.height = Math.floor(innerHeight * dpr);
                                                                      gl.viewport(0, 0, canvas.width, canvas.height);
                                                                    }
                                                                    window.addEventListener('resize', resize);
                                                                    resize();
                                                                  
                                                                    function compile(type, src) {
                                                                      const s = gl.createShader(type);
                                                                      gl.shaderSource(s, src);
                                                                      gl.compileShader(s);
                                                                      if (!gl.getShaderParameter(s, gl.COMPILE_STATUS)) {
                                                                        throw new Error(gl.getShaderInfoLog(s));
                                                                      }
                                                                      return s;
                                                                    }
                                                                  
                                                                    const prog = gl.createProgram();
                                                                    gl.attachShader(prog, compile(gl.VERTEX_SHADER, vs));
                                                                    gl.attachShader(prog, compile(gl.FRAGMENT_SHADER, fs));
                                                                    gl.bindAttribLocation(prog, 0, 'aPos');
                                                                    gl.linkProgram(prog);
                                                                    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) {
                                                                      throw new Error(gl.getProgramInfoLog(prog));
                                                                    }
                                                                    gl.useProgram(prog);
                                                                  
                                                                    // fullscreen triangle
                                                                    const quad = new Float32Array([-1,-1,  3,-1,  -1,3]);
                                                                    const buf = gl.createBuffer();
                                                                    gl.bindBuffer(gl.ARRAY_BUFFER, buf);
                                                                    gl.bufferData(gl.ARRAY_BUFFER, quad, gl.STATIC_DRAW);
                                                                    gl.enableVertexAttribArray(0);
                                                                    gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
                                                                  
                                                                    const iResolution = gl.getUniformLocation(prog, 'iResolution');
                                                                    const iTime = gl.getUniformLocation(prog, 'iTime');
                                                                  
                                                                    let start = performance.now();
                                                                    function frame() {
                                                                      resize(); // accomodate rotation/size change
                                                                      gl.uniform2f(iResolution, canvas.width, canvas.height);
                                                                      gl.uniform1f(iTime, (performance.now() - start) * 0.001);
                                                                      gl.drawArrays(gl.TRIANGLES, 0, 3);
                                                                      requestAnimationFrame(frame);
                                                                    }
                                                                    requestAnimationFrame(frame);
                                                                  }
                                                                  </script>
                                                                  </body>
                                                                  </html>
                                                                  """;
        }
    }
}
