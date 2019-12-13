using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models
{
    public class UpdateFile
    {
        /// <summary>
        /// Nome do sistema que será atualizado
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Nova versão de assembly
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// Informa se a versão é obrigatória
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// URL ou Caminho do arquivo de atualização
        /// </summary>
        public string PathOrURLToFileUpdate { get; set; }

        /// <summary>
        /// Informações sobre arquios e diretórios para manter na atualização
        /// </summary>
        public string[] FilesAndPathToKeep { get; set; }
    }
}
