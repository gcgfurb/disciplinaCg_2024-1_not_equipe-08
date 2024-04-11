﻿//TODO: testar se estes DEFINEs continuam funcionado
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
using System.Diagnostics;
// using OpenTK.Mathematics;

namespace gcgcg
{
    public class Mundo : GameWindow
    {
        private static Objeto mundo = null;

        private char rotuloAtual = '?';
        private Objeto objetoSelecionado = null;

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

            #region Objeto: segmento de reta  
            objetoSelecionado = new SegReta(mundo, ref rotuloAtual, new Ponto4D(0, 0), new Ponto4D(0.25, 0.25));
            #endregion            
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
                if (input.IsKeyPressed(Keys.Q))
                {
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(0).X - 0.05, objetoSelecionado.PontosId(0).Y, 0), 0);
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(1).X - 0.05, objetoSelecionado.PontosId(1).Y, 0), 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                if (input.IsKeyPressed(Keys.W))
                {
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(0).X + 0.05, objetoSelecionado.PontosId(0).Y, 0), 0);
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(1).X + 0.05, objetoSelecionado.PontosId(1).Y, 0), 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                if (input.IsKeyPressed(Keys.S))
                {
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(1).X + 0.05, objetoSelecionado.PontosId(1).Y + 0.05, 0), 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                if (input.IsKeyPressed(Keys.A))
                {
                    objetoSelecionado.PontosAlterar(new Ponto4D(objetoSelecionado.PontosId(1).X - 0.05, objetoSelecionado.PontosId(1).Y - 0.05, 0), 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                #region Fazer girar como relógio
                if (input.IsKeyPressed(Keys.X))
                {
                    Ponto4D vetor = new(objetoSelecionado.PontosId(1).X - objetoSelecionado.PontosId(0).X, objetoSelecionado.PontosId(1).Y - objetoSelecionado.PontosId(0).Y, 0);
                    double raio = Matematica.distancia(objetoSelecionado.PontosId(0), objetoSelecionado.PontosId(1));
                    double aTan = Math.Atan2(vetor.Y, vetor.X);
                    double angulo = aTan * (180 / Math.PI);
                    Ponto4D newPonto = Matematica.GerarPtosCirculo(angulo + 5.0, raio);
                    objetoSelecionado.PontosAlterar(newPonto, 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                if (input.IsKeyPressed(Keys.Z))
                {
                    Ponto4D vetor = new(objetoSelecionado.PontosId(1).X - objetoSelecionado.PontosId(0).X, objetoSelecionado.PontosId(1).Y - objetoSelecionado.PontosId(0).Y, 0);
                    double raio = Matematica.distancia(objetoSelecionado.PontosId(0), objetoSelecionado.PontosId(1));
                    double aTan = Math.Atan2(vetor.Y, vetor.X);
                    double angulo = aTan * (180 / Math.PI);
                    Ponto4D newPonto = Matematica.GerarPtosCirculo(angulo - 5.0, raio);
                    objetoSelecionado.PontosAlterar(newPonto, 1);
                    objetoSelecionado.ObjetoAtualizar();
                }
                #endregion
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
