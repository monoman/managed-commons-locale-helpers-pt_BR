// Class Commons.Locale.Helpers.pt_BR.PorExtenso
//
// Copyright ©2010 Rafael 'Monoman' Teixeira
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Commons.Locale.Helpers.pt_BR
{

	public enum Gênero
	{
		Masculino,
		Feminino

	}

	struct Qualificador
	{
		public readonly decimal Fator;
		public readonly string Singular;
		public readonly string Plural;

		public Qualificador (decimal fator, string singular, string plural)
		{
			Fator = fator;
			Singular = singular;
			Plural = plural;
		}
	}

	///<summary>
	/// Formats a number to it's Brazilian Portuguese written form, allowing for unit and gender variations
	///</summary>
	public class PorExtenso
	{
		static string[] PequenosNúmeros = {"zero", "um", "dois", "três", "quatro", "cinco", "seis", "sete", "oito", "nove", "dez",
			"onze", "doze", "treze", "quatorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove"
		};
		static string[] PequenosNúmerosNoFeminino = {"zero", "uma", "duas", "três", "quatro", "cinco", "seis", "sete", "oito", "nove", "dez",
			"onze", "doze", "treze", "quatorze", "quinze", "dezesseis", "dezessete", "dezoito", "dezenove"
		};
		static string[] Dezenas = { "vinte", "trinta", "quarenta", "cinqüenta", "sessenta", "setenta", "oitenta", "noventa" };
		static string[] Centenas = {"cento", "duzentos", "trezentos", "quatrocentos", "quinhentos", "seiscentos",
			"setecentos", "oitocentos", "novecentos"
		};
		static string[] CentenasNoFeminino = {"cento", "duzentas", "trezentas", "quatrocentas", "quinhentas", "seiscentas",
			"setecentas", "oitocentas", "novecentas"
		};

		static void ProcessarUmFragmento (int valor, List<string> partes, bool precisaDoConector, Gênero gênero)
		{
			if (valor > 0 && valor < 1000) {
				if (precisaDoConector && partes.Count > 0)
					partes.Add("e");
				if (valor > 100) {
					if (gênero == Gênero.Feminino)
						partes.Add(CentenasNoFeminino[(valor / 100) - 1]);
					else
						partes.Add(Centenas[(valor / 100) - 1]);
					precisaDoConector = true;
				} else {
					if (valor == 100) {
						partes.Add("cem");
						return;
					}
					precisaDoConector = false;
				}
				int valorDasDezenas = valor % 100;
				if (valorDasDezenas > 0) {
					if (precisaDoConector)
						partes.Add("e");					
					if (valorDasDezenas < 20) {
						if (gênero == Gênero.Feminino)
							partes.Add(PequenosNúmerosNoFeminino[valorDasDezenas]);
						else
							partes.Add(PequenosNúmeros[valorDasDezenas]);
					} else {
						partes.Add(Dezenas[(valorDasDezenas / 10) - 2]);
						if (valorDasDezenas % 10 > 0) {
							partes.Add("e");					
							if (gênero == Gênero.Feminino)
								partes.Add(PequenosNúmerosNoFeminino[valorDasDezenas % 10]);
							else
								partes.Add(PequenosNúmeros[valorDasDezenas % 10]);
						}
					}
				}
			}
		}

		static Qualificador[] Qualificadores = {
			new Qualificador(1000000000000000000000000m, "septilhão", "septilhões"),
			new Qualificador(1000000000000000000000m, "sextilhão", "sextilhões"),
			new Qualificador(1000000000000000000m, "quintilhão", "quintilhões"),
			new Qualificador(1000000000000000m, "quatrilhão", "quatrilhões"),
			new Qualificador(1000000000000m, "trilhão", "trilhões"),
			new Qualificador(1000000000m, "bilhão", "bilhões"),
			new Qualificador(1000000m, "milhão", "milhões")
		};

		readonly bool useAParteFracionária;
		readonly bool nãoEncurtarUmMilParaMil;
		readonly Gênero gênero;
		readonly string unidadeNoSingular;
		readonly string unidadeNoPlural;
		readonly string unidadeFracionáriaNoSingular;
		readonly string unidadeFracionáriaNoPlural;
		readonly decimal escalaDaParteFracionária;

		public readonly decimal ValorNumérico;

		public static string EmReais (decimal valor)
		{
			PorExtenso formatador = new PorExtenso(valor);
			return formatador.ToString();
		}

		public static string EmDólares (decimal valor)
		{
			PorExtenso formatador = new PorExtenso(valor, "dólar", "dólares", 100m, "centavo de dólar", "centavos de dólar", false, Gênero.Masculino);
			return formatador.ToString();
		}

		public static string CotaçãoDoDólar (decimal valor)
		{
			PorExtenso formatador = new PorExtenso(valor, "real", "reais", 1000m, "milésimo", "milésimos", false, Gênero.Masculino);
			return formatador.ToString();
		}

		public PorExtenso (decimal valor) : this(valor, "real", "reais", 100m, "centavo", "centavos", false, Gênero.Masculino)
		{
		}

		public PorExtenso (decimal valor, string unidadeNoSingular, string unidadeNoPlural, bool useAParteFracionária = true, bool nãoEncurtarUmMilParaMil = false, bool noFeminino = false) :
			this(valor, unidadeNoSingular, unidadeNoPlural, 100m, "centavo", "centavos", nãoEncurtarUmMilParaMil, noFeminino ? Gênero.Feminino : Gênero.Masculino)
		{
			this.useAParteFracionária = useAParteFracionária;
		}

		public PorExtenso (
			decimal valor, 
			string unidadeNoSingular, 
			string unidadeNoPlural,
			decimal escalaDaParteFracionária,
			string unidadeFracionáriaNoSingular,
			string unidadeFracionáriaNoPlural, 
			bool nãoEncurtarUmMilParaMil, 
			Gênero gênero)
		{
			this.ValorNumérico = Math.Abs(valor);
			this.unidadeNoSingular = unidadeNoSingular;
			this.unidadeNoPlural = unidadeNoPlural;
			this.useAParteFracionária = (escalaDaParteFracionária > 0m);
			this.nãoEncurtarUmMilParaMil = nãoEncurtarUmMilParaMil;
			this.gênero = gênero;
			this.escalaDaParteFracionária = escalaDaParteFracionária;
			this.unidadeFracionáriaNoSingular = unidadeFracionáriaNoSingular;
			this.unidadeFracionáriaNoPlural = unidadeFracionáriaNoPlural;
		}

		public override string ToString ()
		{
			return String.Join(" ", Partes);
		}

		public string[] Partes {
			get { 
				if (ValorNumérico == 0m)
					return PequenosNúmeros.Take(1).ToArray();
				var partes = new List<string>();
				decimal valor = ValorNumérico;
				int fragmento;
				foreach (Qualificador qualificador in Qualificadores) {
					if (valor >= qualificador.Fator) {
						fragmento = (int)Math.Truncate(valor / qualificador.Fator);
						ProcessarUmFragmento(fragmento, partes, false, gênero);
						if (fragmento > 1 || partes.Count > 1)
							partes.Add(qualificador.Plural);
						else
							partes.Add(qualificador.Singular);
						valor = valor % qualificador.Fator;
					}						
				}
				if (valor >= 1000m) {
					fragmento = (int)(valor / 1000m);
					if (fragmento != 1 || partes.Count > 0 || nãoEncurtarUmMilParaMil) {
						ProcessarUmFragmento(fragmento, partes, false, gênero);
					}
					partes.Add("mil");
					valor = valor % 1000m;
				}						
				if (valor >= 1m) {
					fragmento = (int)valor;
					if (fragmento == 1 && partes.Count == 0) {
						if (gênero == Gênero.Feminino)
							partes.Add("uma");
						else
							partes.Add("um");
						partes.Add(unidadeNoSingular);
					} else {
						ProcessarUmFragmento(fragmento, partes, true, gênero);
						partes.Add(unidadeNoPlural);
					}
				} else {
					if (ValorNumérico >= 1000000m)
						partes.Add("de");
					if (ValorNumérico >= 2m)
						partes.Add(unidadeNoPlural);
				}						
				if (this.useAParteFracionária) {
					fragmento = (int)((valor % 1m) * escalaDaParteFracionária);
					if (fragmento >= 1) {
						ProcessarUmFragmento(fragmento, partes, true, gênero);
						if (fragmento > 1)
							partes.Add(unidadeFracionáriaNoPlural);
						else
							partes.Add(unidadeFracionáriaNoSingular);
					}
				}
				return partes.ToArray();
			}
		}

		public string[] QuebradoEmLinhas (params int[] tamanhos)
		{
			var linhas = new string[tamanhos.Length];
			var partes = Partes;
			int parteEmQuestão = 0;
			int linhaSendoFormada = 0;
			while (linhaSendoFormada < linhas.Length) {
				var sb = new StringBuilder();
				int size = tamanhos[linhaSendoFormada];
				while (parteEmQuestão < partes.Length && partes[parteEmQuestão].Length + sb.Length <= size) {
					sb.Append(partes[parteEmQuestão++]);
					if (sb.Length < size)
						sb.Append(' ');
				}
				linhas[linhaSendoFormada++] = sb.ToString().Trim();
			}
			if (parteEmQuestão < partes.Length)
				throw new ArgumentException("Tamanhos das linhas são insuficientes para acomodar o texto formatado");
			return linhas;
		}


	}
}
