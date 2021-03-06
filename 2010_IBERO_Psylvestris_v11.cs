using System;
using System.Collections.Generic;
using Simanfor.Core.EngineModels;
namespace EngineTest
{

/// <summary>
/// Todas las funciones y procedimientos son opcionales. Si se elimina cualquiera de ellas, se usará un
/// procedimiento o función por defecto que no modifica el estado del inventario.
/// Modelo IBERO, 2010, Pinus sylvestris (Castilla y León)   ***** Queda por cambiar: cálculo del SI (cuando SI es menor de 15.5) y cálculo del CVd para mortalidad
/// </summary>
public class Template : ModelBase
{
        /// declaracion de variables publicas
        public PieMayor currentTree;

        /// Funciones de perfil utilizadas en el cálculo de volumenes
        public double r2_conCorteza(double HR)
        {
                double r=(1 + 0.4959 * Math.Exp(-14.2598 * HR)) * Math.Pow(0.8474 * currentTree.DAP.Value / 200 * (1 - HR), 0.6312 - 0.6361 * (1 - HR));
            	return Math.Pow(r,2);
        }
        public double r2_sinCorteza(double HR)
        {
	    	double r=(1 + 0.3485 * Math.Exp(-23.9191 * HR)) * Math.Pow(0.7966 * currentTree.DAP.Value / 200 * (1 - HR), 0.6094 - 0.7086 * (1 - HR));
	        return Math.Pow(r,2);
        }

        /// <summary>
        /// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
	/// Solo se ejecuta en el primer nodo. 
	/// Variables que deben permanecer constantes como el índice de sitio deben calcularse solo en este apartado del modelo
        /// </summary>
        /// <param name="plot"></param>
        public override void CalculateInitialInventory(Parcela plot)
        {
            foreach (PieMayor tree in plot.PiesMayores)
            {
                tree.BAL = 0;
                foreach (PieMayor pm in tree.Parcela.PiesMayores)
                {
                    if (pm.DAP>tree.DAP)
                    {
                        tree.BAL+=pm.SEC_NORMAL.Value*pm.EXPAN.Value/10000;
                    }
                }
	if (!tree.ALTURA.HasValue)
	{
	tree.ALTURA = (13 + (27.0392 + 1.4853 * plot.H_DOMINANTE.Value * 10 - 0.1437 * plot.D_CUADRATICO.Value * 10) * Math.Exp(-8.0048 / Math.Sqrt(tree.DAP.Value * 10)) ) / 10;
	}
	if (!tree.ALTURA_MAC.HasValue)
	{ 
	tree.ALTURA_MAC = tree.ALTURA.Value / (1 + Math.Exp((double)(-0.0012*tree.ALTURA.Value*10-0.0102*tree.BAL.Value-0.0168*plot.A_BASIMETRICA.Value )));
	}
	if (!tree.ALTURA_BC.HasValue)
	{ 
	tree.ALTURA_BC = tree.ALTURA_MAC.Value / (1+Math.Exp((double)(1.2425*(plot.A_BASIMETRICA.Value/tree.ALTURA.Value*10) + 0.0047*(plot.A_BASIMETRICA.Value) - 0.5725*Math.Log(plot.A_BASIMETRICA.Value)-0.0082*tree.BAL.Value)));
	}
	tree.CR = (1 - tree.ALTURA_BC/tree.ALTURA);
	if (!tree.LCW.HasValue)
	{ 
	tree.LCW = (0.2518*tree.DAP.Value)*Math.Pow(tree.CR.Value, (0.2386+0.0046*(tree.ALTURA.Value - tree.ALTURA_BC.Value)));
	}
	    currentTree = tree;
	    tree.VCC = Math.PI * tree.ALTURA.Value * IntegralBySimpson(0, 1, 0.01, r2_conCorteza); //Integración --> d_conCorteza sobre HR en los limites 0 -> 1
            tree.VSC = Math.PI * tree.ALTURA.Value * IntegralBySimpson(0,1,0.01,r2_sinCorteza); //Integración --> d_sinCorteza sobre HR en los limites 0 -> 1
            currentTree=null;
            }
	    plot.SI = (plot.H_DOMINANTE.Value * 0.8534446)/Math.Pow((1- Math.Exp((double) (-0.270 * plot.EDAD.Value/10))),2.2779);
        }

/// <summary>
/// Procedimiento que permite la inicialización de variables de parcelas necesarias para la ejecución del modelo
/// </summary>
/// <param name="plot"></param>
public override void Initialize(Parcela plot)
        {
            //plot.SI = (plot.H_DOMINANTE.Value * 0.8534446)/Math.Pow((1- Math.Exp((double) (-0.270 * plot.EDAD.Value/10))),2.2779);
        }

/// <summary>
/// Procedimiento que permite la inicialización de variables de árbol necesarias para la ejecución del modelo
/// </summary>
/// <param name="plot"></param>
/// <param name="tree"></param>
        public override void InitializeTree(Parcela plot, PieMayor tree)
        {
            if (!tree.ESTADO.HasValue || String.IsNullOrEmpty(tree.ESTADO.ToString()))
            {
                tree.BAL = 0;
                foreach (PieMayor pm in tree.Parcela.PiesMayores)
                {
                    if (!pm.ESTADO.HasValue || String.IsNullOrEmpty(pm.ESTADO.ToString()))
                    {
                        if (pm.DAP > tree.DAP)
                        {
                            tree.BAL+=pm.SEC_NORMAL.Value*pm.EXPAN.Value/10000;
                        }
                    }
                }
            }
	if (!tree.ALTURA.HasValue)
	{
	tree.ALTURA = (13 + (27.0392 + 1.4853 * plot.H_DOMINANTE.Value * 10 - 0.1437 * plot.D_CUADRATICO.Value * 10) * Math.Exp(-8.0048 / Math.Sqrt(tree.DAP.Value * 10)) ) / 10;
	}
	if (!tree.ALTURA_MAC.HasValue)
	{
	tree.ALTURA_MAC = tree.ALTURA.Value / (1 + Math.Exp((double)(-0.0012*tree.ALTURA.Value*10-0.0102*tree.BAL.Value-0.0168*plot.A_BASIMETRICA.Value )));
	}
	if (!tree.ALTURA_BC.HasValue)
	{
	tree.ALTURA_BC = tree.ALTURA_MAC.Value / (1+Math.Exp((double)(1.2425*(plot.A_BASIMETRICA.Value/tree.ALTURA.Value*10) + 0.0047*(plot.A_BASIMETRICA.Value) - 0.5725*Math.Log(plot.A_BASIMETRICA.Value)-0.0082*tree.BAL.Value)));
	}
	tree.CR = (1 - tree.ALTURA_BC/tree.ALTURA);
	if (!tree.LCW.HasValue)
	{
	tree.LCW = (0.2518*tree.DAP.Value)*Math.Pow(tree.CR.Value, (0.2386+0.0046*(tree.ALTURA.Value - tree.ALTURA_BC.Value)));
	}
            	currentTree = tree;
                tree.VCC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_conCorteza); //Integración --> r2_conCorteza sobre HR en los limites 0 -> 1
            	tree.VSC=Math.PI*tree.ALTURA.Value*IntegralBySimpson(0,1,0.01,r2_sinCorteza); //Integración --> d_sinCorteza sobre HR en los limites 0 -> 1
            	currentTree = null;
	}

/// <summary>
/// Función que indica si el árbol sobrevive o no después de "years" años
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="tree"></param>
/// <returns>Devuelve el porcentaje de árboles que sobreviven</returns>
public override double Survives(double years, Parcela plot, PieMayor tree)
        {
/// se calcula la desviacion estandar
	double sumOfDerivation = 0;
	double sumOfTrees = 0;
	double DMedioCuadrado=plot.D_MEDIO.Value*plot.D_MEDIO.Value;
	foreach (PieMayor pm in plot.PiesMayores)
	{
		sumOfDerivation += ((pm.DAP.Value) * (pm.DAP.Value) * (pm.EXPAN.Value));
		sumOfTrees += pm.EXPAN.Value;
	}
	double sumOfDerivationAverage = sumOfDerivation / sumOfTrees;
	double StandardDeviation = Math.Sqrt(sumOfDerivationAverage - DMedioCuadrado);

/// calculamos el coeficiente de variación -cvDAP- como el cociente entre la desviacion estandar y la media
	double cvDAP = (StandardDeviation/plot.D_MEDIO.Value);		
	return (1 / (1 + Math.Exp(-6.8548 + (9.792 / tree.DAP.Value) + 0.121 * tree.BAL.Value*cvDAP + 0.037 * plot.SI.Value)));
        ///    return 1;
        }

/// <summary>
/// Procedimiento que permite modificar las propiedades del árbol durante su crecimiento después de "years" años
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="oldTree"></param>
/// <param name="newTree"></param>
public override void Grow(double years, Parcela plot, PieMayor oldTree, PieMayor newTree)
        {
        double DBHG5 = Math.Exp(-0.37110 + 0.2525*Math.Log(oldTree.DAP.Value) + 0.7090 * Math.Log((oldTree.CR.Value + 0.2) / 1.2) + 0.9087 * Math.Log(plot.SI.Value) - 0.1545 * Math.Sqrt(plot.A_BASIMETRICA.Value) - 0.0004 * (oldTree.BAL.Value * oldTree.BAL.Value / Math.Log(oldTree.DAP.Value)));
        newTree.DAP += DBHG5/10;

        double HTG5 = (Math.Exp(3.1222-0.4939*Math.Log(oldTree.DAP.Value)+1.3763*Math.Log(plot.SI.Value)-0.0061*oldTree.BAL.Value+0.1876*Math.Log(oldTree.CR.Value)))/500;
        newTree.ALTURA += HTG5 / 100;
        }

/// <summary>
/// Procedimiento que permite añadir nuevos árboles a una parcela después de "years" años
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <returns>Área basimetrica a distribuir o 0 si no hay masa incorporada</returns>
public override double? AddTree(double years, Parcela plot)
        {
        double result = 1 / (1 + Math.Exp(8.2739 - 0.3022 * plot.D_CUADRATICO.Value));
            if (result >= 0.43F)
            {
                return 5.7855 - 0.1703 * plot.D_CUADRATICO.Value;
            }
            return 0.0F;
        }

/// <summary>
/// Expresa como se ha de distribuir la masa incorporada entre los árboles existentes.
/// La implementación por defecto la distribuye de forma uniforme.
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="AreaBasimetricaIncorporada"></param>
/// <returns></returns>
public override Distribution[] NewTreeDistribution(double years, Parcela plot, double AreaBasimetricaIncorporada)
        {
            Distribution[] distribution = new Distribution[3];
            double percentAreaBasimetrica = AreaBasimetricaIncorporada/plot.A_BASIMETRICA.Value;
            distribution[0] = new Distribution();
			distribution[0].diametroMenor = 0.0;
			distribution[0].diametroMayor = 12.5;
			distribution[0].AreaBasimetricaToAdd = 0.0384 * AreaBasimetricaIncorporada;
            distribution[1] = new Distribution();
			distribution[1].diametroMenor = 12.5; 
			distribution[1].diametroMayor = 22.5;
			distribution[1].AreaBasimetricaToAdd = 0.2718 * AreaBasimetricaIncorporada;
            distribution[2] = new Distribution();
			distribution[2].diametroMenor = 22.5;
			distribution[2].diametroMayor = double.MaxValue;
			distribution[2].AreaBasimetricaToAdd = 0.6898 * AreaBasimetricaIncorporada;
            return distribution;
        }

/// <summary>
/// Procedimiento que realiza todos los precálculos para preparar el procesamiento de los árboles y parcelas.
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="trees"></param>
public override void PreCalculation(double years, Parcela plot, PieMayor[] trees)
        {
        }

/// <summary>
/// Procedimiento que realiza los cálculos sobre un árbol.
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="tree"></param>
public override void ProcessTree(double years, Parcela plot, PieMayor tree)
        {






            double exp = 1 / 0.9718;
            double DIB = Math.Pow((1 / 1.3285), exp) * Math.Pow(tree.DAP.Value*10, exp);
            currentTree = tree;
            tree.VCC = Math.PI * tree.ALTURA.Value * IntegralBySimpson(0,1,0.01,r2_conCorteza);
 //Integración --> d_conCorteza sobre HR en los limites 0 -> 1
            tree.VSC = Math.PI * tree.ALTURA.Value * IntegralBySimpson(0,1,0.01,r2_sinCorteza); //Integración --> d_sinCorteza sobre HR en los limites 0 -> 1
            currentTree = null;
        }

/// <summary>
/// Procedimiento que realiza los cálculos sobre una parcela.
/// </summary>
/// <param name="years"></param>
/// <param name="plot"></param>
/// <param name="trees"></param>
public override void ProcessPlot(double years, Parcela plot, PieMayor[] trees)
        {

        }
    }
}
