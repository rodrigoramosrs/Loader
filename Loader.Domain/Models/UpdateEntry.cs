using Loader.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Models
{
    public class UpdateEntry
    {
        /// <summary>
        /// Nome do sistema que será atualizado
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Versão atual do assembly
        /// </summary>
        public Version CurrentVersion { get; set; }
        /// <summary>
        /// Nova versão de assembly
        /// </summary>
        public Version NewVersion { get; set; }
        /// <summary>
        /// Informa se a versão é obrigatória
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// URL ou Caminho do arquivo de atualização
        /// </summary>
        public string PathOrURLToFileUpdate { get; set; }

        /// <summary>
        /// Informativo se existe ou não atualização
        /// </summary>
        public bool HasUpdate { get; set; }

        /// <summary>
        /// Informações sobre arquios e diretórios para manter na atualização
        /// </summary>
        public string[] FilesAndPathToKeep { get; set; }

        /// <summary>
        /// Instruções de atualizaçao do pacote
        /// </summary>
       public UpdateInstruction UpdateInstruction { get; set; }
    }
}
