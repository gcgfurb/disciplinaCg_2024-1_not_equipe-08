#define CG_Debug

using System;
using System.Runtime;
using CG_Biblioteca;
using OpenTK.Graphics.OpenGL4;

namespace gcgcg
{
  internal class SrPalito : Objeto
  {
    public Ponto4D start { get; set; }
    public Ponto4D end { get; set; }

    private double angulo;
    private double raio;
    private double dislocaX;

    public SrPalito(Objeto _paiRef, ref char _rotulo) : base(_paiRef, ref _rotulo)
    {
      PrimitivaTipo = PrimitiveType.Lines;
      PrimitivaTamanho = 1;
      start = new Ponto4D(0, 0);
      end = new Ponto4D(0.35, 0.35);
      dislocaX = 0;
      angulo = 45;
      raio = Matematica.distancia(start, end);

      base.PontosAdicionar(start);
      base.PontosAdicionar(end);
      base.ObjetoAtualizar();
    }

    private void Atualizar()
    {   
        end = Matematica.GerarPtosCirculo(angulo, raio);
        start.X = dislocaX;
        end.X += dislocaX;
        base.PontosAlterar(start, 0);
        base.PontosAlterar(end, 1);
        base.ObjetoAtualizar();
    }

    public void Movimentar(double value) {
        dislocaX += value;
        Atualizar();    
    }

    public void MudaTamanho(double value) {
        raio += value;
        Atualizar();
    }

    public void Girar(double value) {
        angulo += value;
        Atualizar();
    }


#if CG_Debug
    public override string ToString()
    {
      string retorno;
      retorno = "__ Objeto SrPalito _ Tipo: " + PrimitivaTipo + " _ Tamanho: " + PrimitivaTamanho + "\n";
      retorno += ImprimeToString();
      return (retorno);
    }
#endif

  }
}
