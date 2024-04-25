//TODO: testar se estes DEFINEs continuam funcionado
#define CG_Gizmo  // debugar gráfico.
#define CG_OpenGL // render OpenGL.
// #define CG_DirectX // render DirectX.
// #define CG_Privado // código do professor.

using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
// using OpenTK.Mathematics;

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        private static Objeto mundo = null;

        private char rotuloAtual = '?';
        private char rotuloEspecial = 'X';
        private Objeto objetoSelecionado = null;
        private Poligono poligono = null;
        private Spline spline = null;
        private int idxPoligono = 0;
        private double qtdPontosSpline = 0.1;

        private readonly float[] _sruEixos =
        {
            -0.0f,  0.0f,  0.0f, /* X- */      0.5f,  0.0f,  0.0f, /* X+ */
            0.0f, 0.0f,  0.0f, /* Y- */      0.0f,  0.5f,  0.0f, /* Y+ */
            0.0f,  0.0f, -0.5f, /* Z- */      0.0f,  0.0f,  0.5f, /* Z+ */
        };

        private int _vertexBufferObject_sruEixos;
        private int _vertexArrayObject_sruEixos;

        private Shader _shaderVermelha;
        private Shader _shaderVerde;
        private Shader _shaderAzul;
        //private Vector2 _lastPos;

        public Mundo(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
               : base(gameWindowSettings, nativeWindowSettings)
        {
            mundo ??= new Objeto(null, ref rotuloAtual); //padrão Singleton
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.50196f, 0.50196f, 0.50196f, 1.0f);

            #region Eixos: SRU  
            _vertexBufferObject_sruEixos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject_sruEixos);
            GL.BufferData(BufferTarget.ArrayBuffer, _sruEixos.Length * sizeof(float), _sruEixos, BufferUsageHint.StaticDraw);
            _vertexArrayObject_sruEixos = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            _shaderVermelha = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
            _shaderVerde = new Shader("Shaders/shader.vert", "Shaders/shaderVerde.frag");
            _shaderAzul = new Shader("Shaders/shader.vert", "Shaders/shaderAzul.frag");
            #endregion

            List<Ponto4D> pontosPoligono = new List<Ponto4D>();
            pontosPoligono.Add(new Ponto4D(0.5, -0.5));// 0
            pontosPoligono.Add(new Ponto4D(0.5, 0.5));// 1
            pontosPoligono.Add(new Ponto4D(-0.5, 0.5));// 2 
            pontosPoligono.Add(new Ponto4D(-0.5, -0.5));// 3
            poligono = new Poligono(mundo, ref rotuloEspecial, pontosPoligono)
            {
                PrimitivaTipo = PrimitiveType.LineStrip,
                shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderCiano.frag"),
            };

            #region Objeto: ponto  
            objetoSelecionado = new Ponto(mundo, ref rotuloAtual, new Ponto4D(0.5, 0.5)); // 1
            objetoSelecionado = new Ponto(mundo, ref rotuloAtual, new Ponto4D(-0.5, 0.5)); // 2
            objetoSelecionado = new Ponto(mundo, ref rotuloAtual, new Ponto4D(-0.5, -0.5)); // 3
            objetoSelecionado = new Ponto(mundo, ref rotuloAtual, new Ponto4D(0.5, -0.5))//0
            {
                shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag"),
            };
            #endregion

            spline = new Spline(poligono, ref rotuloEspecial)
            {
                shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderAmarela.frag"),
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

#if CG_Gizmo
            Sru3D();
#endif
            mundo.Desenhar();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            #region Teclado
            var input = KeyboardState;
            if (input.IsKeyPressed(Keys.Escape))
            {
                Close();
            }
            else
            {
                if (input.IsKeyPressed(Keys.Space))
                {
                    if (objetoSelecionado == null) {
                        objetoSelecionado = mundo;
                        return;
                    }
                    objetoSelecionado.shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderBranca.frag");
                    objetoSelecionado = mundo.GrafocenaBuscaProximo(objetoSelecionado);
                    objetoSelecionado.shaderObjeto = new Shader("Shaders/shader.vert", "Shaders/shaderVermelha.frag");
                    
                    idxPoligono++;
                    if (idxPoligono > 3) idxPoligono = 0;
                }
                if (input.IsKeyPressed(Keys.C))
                {
                    Ponto4D newPonto = new Ponto4D(objetoSelecionado.PontosId(0).X, objetoSelecionado.PontosId(0).Y + 0.05);
                    objetoSelecionado.PontosAlterar(newPonto, 0);
                    poligono.PontosAlterar(newPonto, idxPoligono);
                    spline.Atualizar();
                }
                if (input.IsKeyPressed(Keys.B))
                {
                    Ponto4D newPonto = new Ponto4D(objetoSelecionado.PontosId(0).X, objetoSelecionado.PontosId(0).Y - 0.05);
                    objetoSelecionado.PontosAlterar(newPonto, 0);
                    poligono.PontosAlterar(newPonto, idxPoligono);
                    spline.Atualizar();
                }
                 if (input.IsKeyPressed(Keys.D))
                {
                    Ponto4D newPonto = new Ponto4D(objetoSelecionado.PontosId(0).X + 0.05, objetoSelecionado.PontosId(0).Y);
                    objetoSelecionado.PontosAlterar(newPonto, 0);
                    poligono.PontosAlterar(newPonto, idxPoligono);
                    spline.Atualizar();
                }
                if (input.IsKeyPressed(Keys.E))
                {
                    Ponto4D newPonto = new Ponto4D(objetoSelecionado.PontosId(0).X - 0.05, objetoSelecionado.PontosId(0).Y);
                    objetoSelecionado.PontosAlterar(newPonto, 0);
                    poligono.PontosAlterar(newPonto, idxPoligono);
                    spline.Atualizar();
                }
                if (input.IsKeyPressed(Keys.KeyPadAdd))
                {
                    qtdPontosSpline -= 0.01;
                    if (qtdPontosSpline <= 0) qtdPontosSpline = 0.01;
                    spline.SplineQtdPto(qtdPontosSpline);
                }
                if (input.IsKeyPressed(Keys.Minus) || input.IsKeyPressed(Keys.KeyPadSubtract))	
                {
                    qtdPontosSpline += 0.01;
                    spline.SplineQtdPto(qtdPontosSpline);
                }
            }
            #endregion
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            mundo.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject_sruEixos);
            GL.DeleteVertexArray(_vertexArrayObject_sruEixos);

            GL.DeleteProgram(_shaderVermelha.Handle);
            GL.DeleteProgram(_shaderVerde.Handle);
            GL.DeleteProgram(_shaderAzul.Handle);

            base.OnUnload();
        }

#if CG_Gizmo
        private void Sru3D()
        {
#if CG_OpenGL && !CG_DirectX
            GL.BindVertexArray(_vertexArrayObject_sruEixos);
            // EixoX
            _shaderVermelha.Use();
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            // EixoY
            _shaderVerde.Use();
            GL.DrawArrays(PrimitiveType.Lines, 2, 2);
            // EixoZ
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
