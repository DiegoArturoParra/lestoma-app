using CsvHelper.Configuration.Attributes;
using lestoma.CommonUtils.Enums;
using lestoma.CommonUtils.Requests.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace lestoma.CommonUtils.DTOs
{
    public class ReporteDTO
    {
        public string UserGenerator { get; set; }
        public List<ReportInfo> Reporte { get; set; }
        public DateFilterRequest FiltroFecha { get; set; }
    }

    public class ReportInfo
    {
        [DisplayName("Nombre UPA")]
        public string NombreUpa { get; set; }
        [DisplayName("Usuario")]
        public string Usuario { get; set; }
        [DisplayName("Fecha de Servidor")]
        public DateTime? FechaServidor { get; set; }
        [DisplayName("Fecha de Dispositivo")]
        public DateTime FechaDispositivo { get; set; }
        [DisplayName("Componente")]
        public string Componente { get; set; }
        [DisplayName("Módulo")]
        public string Modulo { get; set; }
        [DisplayName("Tipo de función")]
        public string Estado { get; set; }
        
        [Ignore]
        [DisplayName("Trama de Entrada")]
        public string TramaIn { get; set; }

        [Ignore]
        [DisplayName("Resultado Trama de Entrada")] 
        public double? ResultTramaIn { get; set; }

        [Ignore]
        [DisplayName("Trama de Salida")]
        public string TramaOut { get; set; }

        [Ignore]
        [DisplayName("Resultado Trama de Salida")]
        public double? ResultTramaOut { get; set; }

        [DisplayName("Estado Inicial")]
        public string ResultStatusInitial => GetStatusInitial(ResultTramaIn);
        [DisplayName("Estado Final")]
        public string ResultStatusFinal => GetStatusFinal(ResultTramaOut, TramaOut);
        [DisplayName("Set Point")]
        public string ResultSetPoint => GetSetPointOut(ResultTramaIn, TramaOut);

        private string GetStatusInitial(double? resultTramaIn)
        {
            if (resultTramaIn.HasValue && Estado != EnumConfig.GetDescription(TipoEstadoComponente.Ajuste))
            {
                return resultTramaIn.Value switch
                {
                    0 => Constants.Constants.APAGADO,
                    1 => Constants.Constants.ENCENDIDO,
                    _ => string.Empty,
                };
            }
            else
            {
                return string.Empty;
            }
        }
        private string GetStatusFinal(double? resultTramaOut, string tramaOut)
        {
            if (tramaOut.Equals(Constants.Constants.TRAMA_SUCESS))
            {
                return HttpStatusCode.OK.ToString();
            }
            else if (resultTramaOut.HasValue)
            {
                return resultTramaOut.Value switch
                {
                    0 => Constants.Constants.APAGADO,
                    1 => Constants.Constants.ENCENDIDO,
                    _ => resultTramaOut.Value.ToString(),
                };
            }
            else { return string.Empty; }
        }
        private string GetSetPointOut(double? valorSetPointEnviado, string tramaRecibida)
        {
            string valor = "N/A";
            if (tramaRecibida.Equals(Constants.Constants.TRAMA_SUCESS))
            {
                valor = valorSetPointEnviado.ToString();
            }
            return valor;
        }
    }
}
