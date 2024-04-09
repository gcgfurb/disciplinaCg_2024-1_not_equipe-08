using System;
using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;

namespace gcgcg
{
    internal class Circulo : Objeto
    {
        public Circulo(Objeto paiRef, ref char rotulo, double tamanho) : base(paiRef, ref rotulo)
        {
            PrimitivaTipo = PrimitiveType.Points;
            Ponto4D ponto = new();
            for (int i = 0; i < 360; i += 5) {
                ponto = Matematica.GerarPtosCirculo(i, tamanho);
                base.PontosAdicionar(ponto);
            }
            base.ObjetoAtualizar();        
        }

        #if CG_Debug
        public override string ToString()
        {
        string retorno;
        retorno = "__ Objeto Círculo _ Tipo: " + PrimitivaTipo + "\n";
        retorno += base.ToString();
        return (retorno);

        }
        #endif
    }
}