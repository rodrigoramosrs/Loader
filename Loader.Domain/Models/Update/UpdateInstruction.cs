using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Loader.Domain.Models.Update
{
    public class UpdateInstruction
    {

        /// <summary>
        /// ID de controle de registro
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Nome da aplicação
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Diretório de tabalho da aplicação, que será atualizada
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Diretório onde serão salvas as versões após a atualização
        /// </summary>
        public string BackupVersionDirectory { get; set; }

        /// <summary>
        /// Caminho completo da dll ou executavel para validação da versão
        /// </summary>
        public string MainAssembly { get; set; }

        /// <summary>
        /// Id da aplicação no IIS
        /// </summary>
        public string IISAppID { get; set; }

        /// <summary>
        /// Linha de comando que deve ser executada antes da atualização
        /// </summary>
        public string CommandLineBeforeUpdate { get; set; }

        /// <summary>
        /// Linha de comando que deve ser executada apos a atualização.
        /// </summary>
        public string CommandLineAfterUpdate { get; set; }

        /// <summary>
        /// Informa se após a atualização deve ser feita a validação da versão.
        /// </summary>
        public bool CheckVersionAfterUpdate { get; set; }

        /// <summary>
        /// Informa se o sistema deve buscar ou não por atualizações automaticamente
        /// </summary>
        public bool AutoUpdate { get; set; }

       
        /// <summary>
        /// URL ou Caminho no qual o sistema vai buscar informação sobre as atualizações
        /// </summary>
        public string UrlOrPathToUpdateDefinition { get; set; }

        /// <summary>
        /// Metodo reponsável por converter os parametros da linha de comando por valores das propriedades
        /// </summary>
        public string GetCommandLineBeforeUpdateWithReplacedParams()
        {
            string command = this.CommandLineBeforeUpdate;

            if (command.EndsWith(".bat") || command.EndsWith(".sh"))
                command = File.ReadAllText(this.CommandLineBeforeUpdate);

            return this.DoReplaceParamsWithInternalData(command);
        }
        /// <summary>
        /// Metodo reponsável por converter os parametros da linha de comando por valores das propriedades
        /// </summary>
        public string GetCommandLineAfterUpdateWithReplacedParams()
        {
            string command = this.CommandLineAfterUpdate;

            if (command.EndsWith(".bat") || command.EndsWith(".sh"))
                command = File.ReadAllText(this.CommandLineAfterUpdate);

            return this.DoReplaceParamsWithInternalData(command);
        }

        private string DoReplaceParamsWithInternalData(string param)
        {
            List<string> ExclusionList = new List<string>() { "CommandLineBeforeUpdate", "_CommandLineBeforeUpdate", "CommandLineAfterUpdate", "_CommandLineAfterUpdate" };
            string returnData = param;

            

            foreach (var property in this.GetType().GetProperties())
            {
                if (ExclusionList.Contains(property.Name)) continue;

                object PropertyValue = property.GetValue(this);
                returnData = returnData.Replace($"%{property.Name}%", (property.GetValue(this) ?? "").ToString());

            }

            return returnData;
        }
    }
}
