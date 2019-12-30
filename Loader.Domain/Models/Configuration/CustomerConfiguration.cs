using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models.Configuration
{
    public class CustomerConfiguration
    {
        public string ID { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Configuração responsável por informar se deve validar as licenças de uso dos softwares configurados no atualizador
        /// </summary>
        public bool CheckLicense { get; set; }
    }
}
