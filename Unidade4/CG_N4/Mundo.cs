﻿#define CG_Gizmo // debugar gráfico.
#define CG_OpenGL // render OpenGL.
// #define CG_DirectX // render DirectX.
// #define CG_Privado // código do professor.

using System;
using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;

//FIXME: padrão Singleton

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        Objeto mundo;
        private char rotuloNovo = '?';
        private Objeto objetoSelecionado = null;

        private readonly float[] _sruEixos =
        {
            -0.5f, 0.0f, 0.0f, /* X- */ 0.5f, 0.0f, 0.0f, /* X+ */
            0.0f, -0.5f, 0.0f, /* Y- */ 0.0f, 0.5f, 0.0f, /* Y+ */
            0.0f, 0.0f, -0.5f, /* Z- */ 0.0f, 0.0f, 0.5f /* Z+ */
        };

        private int _vertexBufferObject_sruEixos;
        private int _vertexArrayObject_sruEixos;

        private int _elementBufferObject;
        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private readonly float[] _vertices =
        {
            // frente
            -1, -1, 1, 0, 0,
             1, -1, 1, 1, 0,
             1,  1, 1, 1, 1,
            -1,  1, 1, 0, 1,
            // trás
            -1, -1, -1, 0, 0,
             1, -1, -1, 1, 0,
             1,  1, -1, 1, 1,
            -1,  1, -1, 0, 1,
            // direita
            1, -1,  1, 0, 0,
            1, -1, -1, 1, 0,
            1,  1, -1, 1, 1,
            1,  1,  1, 0, 1,
            // esquerda
            -1, -1,  1, 0, 0,
            -1, -1, -1, 1, 0,
            -1,  1, -1, 1, 1,
            -1,  1,  1, 0, 1,
            // superior
            -1, 1,  1, 0, 0,
             1, 1,  1, 1, 0,
             1, 1, -1, 1, 1,
            -1, 1, -1, 0, 1,
            // inferior
            -1, -1,  1, 0, 0,
             1, -1,  1, 1, 0,
             1, -1, -1, 1, 1,
            -1, -1, -1, 0, 1
        };

        private readonly uint[] _indices =
        {
            0, 1, 2, 2, 3, 0, // frente
            4, 5, 6, 6, 7, 4, // trás
            8, 9, 10, 10, 11, 8, // direita
            12, 13, 14, 14, 15, 12, // esquerda
            16, 17, 18, 18, 19, 16, // superior
            20, 21, 22, 22, 23, 20 // inferior
        };

        private Shader _shaderBranca;
        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;
        private Shader _shaderCiano;
        private Shader _shaderMagenta;
        private Shader _shaderAmarela;
        private Shader _shader;

        private Texture _texture;
        private Camera _camera;
        private Cubo cubo;
        private Ponto cuboMenor;

        private bool _firstMove = true;
        private Vector2 _lastPos;
        
        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
           : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo ??= new Objeto(null, ref rotuloNovo); //padrão Singleton
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            Utilitario.Diretivas();

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            GL.Enable(EnableCap.DepthTest); // Ativar teste de profundidade
            GL.Enable(EnableCap.CullFace); // Desenha os dois lados da face
            // GL.FrontFace(FrontFaceDirection.Cw);
            // GL.CullFace(CullFaceMode.FrontAndBack);

            #region Cores

            _shaderBranca = new Shader("Shaders/shader.vert", "Shaders/shaderBranca.frag");
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");
            _shaderCiano = new Shader("Shaders/shader.vert", "Shaders/shaderCiano.frag");
            _shaderMagenta = new Shader("Shaders/shader.vert", "Shaders/shaderMagenta.frag");
            _shaderAmarela = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag");

            #endregion

            #region Eixos: SRU

            _vertexBufferObject_sruEixos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_sruEixos);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos,
                BufferUsageHint.StaticDraw);
            _vertexArrayObject_sruEixos = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            #endregion

            #region Objeto: cubo  
            cubo = new Cubo(mundo, ref rotuloNovo);
            objetoSelecionado = cubo;
            #endregion

            #region Objeto: ponto  
            cuboMenor = new Ponto(cubo, ref rotuloNovo, new Ponto4D(2.0, 0.0));
            cuboMenor.PrimitivaTipo = PrimitiveType.Points;
            cuboMenor.PrimitivaTamanho = 10;
        
            #endregion

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices,
                BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/lighting.vert", "Shaders/lighting.frag");
            _shader.Use();

            var vertexLocation = _shader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            _texture = Texture.LoadFromFile("Imagens/image.jpg");
            _texture.Use(TextureUnit.Texture0);

            _camera = new Camera(Vector3.UnitZ * 5, Size.X / (float)Size.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // bind pra mostrar a textura
            GL.BindVertexArray(_vertexArrayObject);

            // faz a textura aparecer em todos os momentos
            _shader.Use();

            // por algum motivo, sem isso a textura nao aparece, TODO ver.
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            objetoSelecionado.shaderCor = _shader;

            cuboMenor.MatrizRotacao(0.02);

            mundo.Desenhar(new Transformacao4D(), _camera);

#if CG_Gizmo
            Gizmo_Sru3D();
#endif
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // ☞ 396c2670-8ce0-4aff-86da-0f58cd8dcfdc   TODO: forma otimizada para teclado.
            #region Teclado
            var estadoTeclado  = KeyboardState;
            if (estadoTeclado .IsKeyDown(Keys.Escape))
                Close();
            if (estadoTeclado .IsKeyPressed(Keys.Space))
            {
                if (objetoSelecionado == null)
                    objetoSelecionado = mundo;
                objetoSelecionado.shaderCor = _shaderBranca;
                objetoSelecionado = mundo.GrafocenaBuscaProximo(objetoSelecionado);
                objetoSelecionado.shaderCor = _shaderAmarela;
            }
            if (estadoTeclado.IsKeyPressed(Keys.G))
                mundo.GrafocenaImprimir("");
            if (estadoTeclado.IsKeyPressed(Keys.P) && objetoSelecionado != null)
                Console.WriteLine(objetoSelecionado.ToString());
            if (estadoTeclado.IsKeyPressed(Keys.M) && objetoSelecionado != null)
                objetoSelecionado.MatrizImprimir();
            if (estadoTeclado.IsKeyPressed(Keys.I) && objetoSelecionado != null)
                objetoSelecionado.MatrizAtribuirIdentidade();
            if (estadoTeclado.IsKeyDown(Keys.Left) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(-0.005, 0, 0);
            if (estadoTeclado.IsKeyDown(Keys.Right) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0.005, 0, 0);
            if (estadoTeclado.IsKeyDown(Keys.Up) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0.005, 0);
            if (estadoTeclado.IsKeyDown(Keys.Down) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, -0.005, 0);
            if (estadoTeclado.IsKeyPressed(Keys.O) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0, 0.05);
            if (estadoTeclado.IsKeyPressed(Keys.L) && objetoSelecionado != null)
                objetoSelecionado.MatrizTranslacaoXYZ(0, 0, -0.05);
            if (estadoTeclado.IsKeyPressed(Keys.PageUp) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(2, 2, 2);
            if (estadoTeclado.IsKeyPressed(Keys.PageDown) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZ(0.5, 0.5, 0.5);
            if (estadoTeclado.IsKeyPressed(Keys.Home) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(0.5, 0.5, 0.5);
            if (estadoTeclado.IsKeyPressed(Keys.End) && objetoSelecionado != null)
                objetoSelecionado.MatrizEscalaXYZBBox(2, 2, 2);

            if (estadoTeclado.IsKeyPressed(Keys.D1))
            {
               
            }

            if (estadoTeclado.IsKeyPressed(Keys.D2))
            {
                
            }

            if (estadoTeclado.IsKeyPressed(Keys.D3))
            {
                
            }

            if (estadoTeclado.IsKeyPressed(Keys.D4))
            {
                
            }

            if (estadoTeclado.IsKeyPressed(Keys.D5))
            {
                
            }

            if (estadoTeclado.IsKeyPressed(Keys.D6))
            {
                
            }

            if (estadoTeclado.IsKeyPressed(Keys.D0))
            {
               
            }

            const float cameraSpeed = 1.5f;
            if (estadoTeclado.IsKeyDown(Keys.Z))
                _camera.Position = Vector3.UnitZ * 5;
            if (estadoTeclado.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            if (estadoTeclado.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            if (estadoTeclado.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            if (estadoTeclado.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            if (estadoTeclado.IsKeyDown(Keys.RightShift))
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            if (estadoTeclado.IsKeyDown(Keys.LeftShift))
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            if (estadoTeclado.IsKeyDown(Keys.D9))
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up

            #endregion

            #region  Mouse

            if (MouseState.IsButtonDown(MouseButton.Left))
            {
                const float sensitivity = 0.2f;
                var mouse = MouseState;

                    if (_firstMove)
                    {
                        _lastPos = new Vector2(mouse.X, mouse.Y);
                        _firstMove = false;
                    }
                    else
                    {
                        var deltaX = mouse.X - _lastPos.X;
                        var deltaY = mouse.Y - _lastPos.Y;
                        _lastPos = new Vector2(mouse.X, mouse.Y);

        
                        _camera.Yaw += deltaX * sensitivity;
                        _camera.Pitch -= deltaY * sensitivity; 
                    }
            }
            if (MouseState.IsButtonDown(MouseButton.Right) && objetoSelecionado != null)
            {
                Console.WriteLine("MouseState.IsButtonDown(MouseButton.Right)");

                int janelaLargura = ClientSize.X;
                int janelaAltura = ClientSize.Y;
                Ponto4D mousePonto = new Ponto4D(MousePosition.X, MousePosition.Y);
                Ponto4D sruPonto = Utilitario.NDC_TelaSRU(janelaLargura, janelaAltura, mousePonto);

                objetoSelecionado.PontosAlterar(sruPonto, 0);
            }
            if (MouseState.IsButtonReleased(MouseButton.Right))
            {
                Console.WriteLine("MouseState.IsButtonReleased(MouseButton.Right)");
            }

            #endregion
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            #if CG_DEBUG      
                Console.WriteLine("Tamanho interno da janela de desenho: " + ClientSize.X + "x" + ClientSize.Y);
            #endif
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        }

        protected override void OnUnload()
        {
            mundo.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject_sruEixos);
            GL.DeleteVertexArray(_vertexArrayObject_sruEixos);

            GL.DeleteProgram(_shaderBranca.Handle);
            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);
            GL.DeleteProgram(_shaderCiano.Handle);
            GL.DeleteProgram(_shaderMagenta.Handle);
            GL.DeleteProgram(_shaderAmarela.Handle);
            GL.DeleteProgram(_shader.Handle);

            base.OnUnload();
        }

#if CG_Gizmo
        private void Gizmo_Sru3D()
        {
#if CG_OpenGL && !CG_DirectX
            var model = Matrix4.Identity;
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            // EixoX
            _shaderVermelha.SetMatrix4("model", model);
            _shaderVermelha.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderVermelha.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _shaderVermelha.Use();
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            // EixoY
            _shaderVerde.SetMatrix4("model", model);
            _shaderVerde.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderVerde.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _shaderVerde.Use();
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            // EixoZ
            _shaderAzul.SetMatrix4("model", model);
            _shaderAzul.SetMatrix4("view", _camera.GetViewMatrix());
            _shaderAzul.SetMatrix4("projection", _camera.GetProjectionMatrix());
            _shaderAzul.Use();
            GL.DrawArrays(PrimitiveType.Lines, 4, 2);
#elif CG_DirectX && !CG_OpenGL
      Console.WriteLine(" .. Coloque aqui o seu código em DirectX");
#elif (CG_DirectX && CG_OpenGL) || (!CG_DirectX && !CG_OpenGL)
      Console.WriteLine(" .. ERRO de Render - escolha OpenGL ou DirectX !!");
#endif
        }
#endif
    }
}