using Hangfire;
using Loader.Domain.Entities;
using Loader.Domain.Interfaces;
using Loader.Domain.Models;
using Loader.Infra.Manager;
using Loader.Infra.Manager.Bridge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Loader.Service.Services
{
    public class UpdateService
    {
        private readonly IUpdateRepository _UpdateRepository;
        public UpdateService(IUpdateRepository updateRepository)
        {
            _UpdateRepository = updateRepository;
        }

        public UpdateEntry HasUpdate(UpdateInstruction UpdateInstruction)
        {
            //Verifica se existe atualização para o sistema informado
            //Caso exista atualizar o retorno com dados necessários para atualizaçõ
            //Caso não exista, retornar que não existe atualização
            string UpdateEntryString = DownloadManager.DownloadString(UpdateInstruction.UrlOrPathToUpdateDefinition);

            UpdateFile updateFile = JsonConvert.DeserializeObject<UpdateFile>(UpdateEntryString);

            System.Version CurrentVersion = AssemblyManager.GetAssemblyVersion(UpdateInstruction.MainAssembly);
            bool IsNewerVersion = AssemblyManager.IsNewerVersion(CurrentVersion, updateFile.Version);

            return new UpdateEntry()
            {
                PathOrURLToFileUpdate = updateFile.PathOrURLToFileUpdate,
                CurrentVersion = CurrentVersion,
                NewVersion = updateFile.Version,
                FilesAndPathToKeep = updateFile.FilesAndPathToKeep,
                HasUpdate = IsNewerVersion,
                IsMandatory = false,
                ProductName = UpdateInstruction.Name,
                UpdateInstruction = UpdateInstruction
            };
        }

        [AutomaticRetry(Attempts = 0)]
        public UpdateResult DoUpdate(UpdateInstruction UpdateInstruction)
        {
            UpdateEntry updateEntry = this.HasUpdate(UpdateInstruction);
            if (!updateEntry.HasUpdate)
            {
                var updateResultReturn = new UpdateResult()
                {
                    ID = new Guid(),
                    IsSuccess = false,
                    //Message = "No update found. Nothing to update!",
                    UpdateInstructionID = UpdateInstruction.ID
                };
                updateResultReturn.AddMessage("No update found. Nothing to update!", UpdateResultMessage.eMessageType.INFORMATION);
                this._UpdateRepository.WriteUpdateInstructionResult(updateResultReturn);
                return updateResultReturn;
            }    

            return ExecuteUpdate(updateEntry);
        }

        public UpdateResult ExecuteUpdate(UpdateEntry UpdateEntry)
        {
            //1 - Rodar linha de comando antes da atualização
            //2 - Fazer backup do diretório antigo com prefixo da versão
            //3 - Criar novo diretório
            //4 - Descompactar os arquivos
            //5 - Valida os arquivos e diretórios que devem permanecer e faço a copia para o novo diretório após a atualização
            //6 - Rodar linha de comano pós update
            //7 - Validando a versão após a atualização para verificar se está correta.
            var updateResultReturn = new UpdateResult()
            {
                ID = Guid.NewGuid(),
                UpdateInstructionID = UpdateEntry.UpdateInstruction.ID
            };
            Stopwatch stopWatchTimer = Stopwatch.StartNew();
            
            updateResultReturn.AddMessage("Starting the update process", UpdateResultMessage.eMessageType.INFORMATION);
            var comandLineBeforeResult = Shell.ExecuteTerminalCommand(UpdateEntry.UpdateInstruction.CommandLineBeforeUpdate);
            if (comandLineBeforeResult.code > 0) throw new Exception($"Cannot run Commandline before update, see details: \r\n{comandLineBeforeResult.stdout}");
            updateResultReturn.AddMessage($"Command line 'CommandLineBeforeUpdate' executed.\r\nOutput code: {comandLineBeforeResult.code}\r\nOutput data:{comandLineBeforeResult.stdout}", UpdateResultMessage.eMessageType.SUCCESS);

            //Executando backup da pasta atual
            string workingFolderBackup = this.GenerateBackupFolderFullPathName(UpdateEntry.UpdateInstruction.ID, updateResultReturn.ID, UpdateEntry.UpdateInstruction.WorkingDirectory, UpdateEntry.CurrentVersion);
            //string BackupSufix = $"{ DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss") }_{ UpdateEntry.CurrentVersion}";
            //string workingFolderBackup = $"{UpdateEntry.UpdateInstruction.WorkingDirectory}_{BackupSufix}";
            bool backupDone = DirectoryManager.RenameDirectory(UpdateEntry.UpdateInstruction.WorkingDirectory, workingFolderBackup);
            if (!backupDone) throw new Exception($"Cannot create backup folder.");
            updateResultReturn.AddMessage($"Backup the current version was succeeded. Folder created is {workingFolderBackup}", UpdateResultMessage.eMessageType.SUCCESS);

            // Criando diretório após mover o anterior
            Directory.CreateDirectory(UpdateEntry.UpdateInstruction.WorkingDirectory);
            updateResultReturn.AddMessage($"Creation of new folder for update was succeeded. Folder created is {UpdateEntry.UpdateInstruction.WorkingDirectory}", UpdateResultMessage.eMessageType.SUCCESS);

            //Descompactando os arquivos para a pasta nova
            string ZipUpdateFilePathOrUrl = UpdateEntry.PathOrURLToFileUpdate;
            bool decompressed = ZipFileManger.Decompress(ZipUpdateFilePathOrUrl, UpdateEntry.UpdateInstruction.WorkingDirectory);
            if (!decompressed) throw new Exception($"Cannot decompress file '{ZipUpdateFilePathOrUrl}' into folder '{UpdateEntry.UpdateInstruction.WorkingDirectory}'");
            updateResultReturn.AddMessage($"Unzip update file was succeeded. Update file of path is {ZipUpdateFilePathOrUrl}, its decompressed at folder {UpdateEntry.UpdateInstruction.WorkingDirectory}", UpdateResultMessage.eMessageType.SUCCESS);

            //Valida os arquivos e diretórios que devem permanecer e faço a copia para o novo diretório após a atualização
            updateResultReturn.AddMessage($"Preparing to start copy of 'FilesAndPathToKeep'. The number of files and path to keep is {UpdateEntry.FilesAndPathToKeep.Length}", UpdateResultMessage.eMessageType.INFORMATION);
            foreach (var FileOrFolder in UpdateEntry.FilesAndPathToKeep)
            {
                string SourcelFile = Path.Combine($"{workingFolderBackup}", $"{FileOrFolder}");
                string DestinationFile = Path.Combine($"{UpdateEntry.UpdateInstruction.WorkingDirectory}", $"{FileOrFolder}");

                bool copyResult = DirectoryManager.CopyFilesOrFolder(SourcelFile, DestinationFile);

                updateResultReturn.AddMessage($"File/Folder copy from '{SourcelFile}' to '{DestinationFile}' " + (copyResult ? " succeeded" : " does not succeeded"),
                   copyResult ? UpdateResultMessage.eMessageType.SUCCESS : UpdateResultMessage.eMessageType.ERROR);

            }


            var comandLineAfterResult = Shell.ExecuteTerminalCommand(UpdateEntry.UpdateInstruction.CommandLineAfterUpdate);
            if (comandLineAfterResult.code > 0) throw new Exception($"Cannot run Commandline after update, see details: \r\n{comandLineAfterResult.stdout}");
            updateResultReturn.AddMessage($"Command line 'CommandLineBeforeUpdate' executed.\r\nOutput code: {comandLineAfterResult.code}\r\nOutput data:{comandLineAfterResult.stdout}", UpdateResultMessage.eMessageType.SUCCESS);

            //Validando a versão após todo o processo para garantir que a versão está correta.
            if (UpdateEntry.UpdateInstruction.CheckVersionAfterUpdate)
            {
                System.Version CurrentVersion = AssemblyManager.GetAssemblyVersion(UpdateEntry.UpdateInstruction.MainAssembly);
                bool IsNewerVersion = AssemblyManager.IsNewerVersion(CurrentVersion, UpdateEntry.NewVersion);
                if (UpdateEntry.NewVersion != CurrentVersion)
                    throw new Exception($"Version validation  after update does not pass. Expected version is '{ UpdateEntry.NewVersion }' but found '{CurrentVersion}'");
                updateResultReturn.AddMessage($"Version validation after update passed", UpdateResultMessage.eMessageType.SUCCESS);
            }
            else
                updateResultReturn.AddMessage($"Version validation is not executed. CheckVersionAfterUpdate is false", UpdateResultMessage.eMessageType.INFORMATION);


            updateResultReturn.IsSuccess = true;

            stopWatchTimer.Stop();
            updateResultReturn.TimeSpentMilliseconds = stopWatchTimer.ElapsedMilliseconds;

            string alepsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        stopWatchTimer.Elapsed.Hours,
                        stopWatchTimer.Elapsed.Minutes,
                        stopWatchTimer.Elapsed.Seconds,
                        stopWatchTimer.Elapsed.Milliseconds);

            updateResultReturn
                .AddMessage($"Updated '{UpdateEntry.UpdateInstruction.Name}' from '{UpdateEntry.CurrentVersion}' to '{UpdateEntry.NewVersion}'. Alepsed time: {alepsedTime}",
                            updateResultReturn.IsSuccess ? UpdateResultMessage.eMessageType.SUCCESS : UpdateResultMessage.eMessageType.ERROR);

            this._UpdateRepository.WriteUpdateInstructionResult(updateResultReturn);

            return updateResultReturn;


        }

        

        public UpdateResult DoRollback(UpdateInstruction UpdateInstruction, UpdateBackupEntry UpdateBackupEntry)
        {
            //1 - Rodar linha de comando antes do rollback
            //2 - Fazer backup do diretório antigo com prefixo da versão
            //3 - Renomear Diretório do backup para o diretório atual
            //4 - Rodar linha de comano pós rollback

            var updateResultReturn = new UpdateResult()
            {
                ID = Guid.NewGuid(),
                UpdateInstructionID = UpdateInstruction.ID
            };

            Stopwatch stopWatchTimer = Stopwatch.StartNew();
            updateResultReturn.AddMessage("Starting the update process", UpdateResultMessage.eMessageType.INFORMATION);
            System.Version CurrentVersion = AssemblyManager.GetAssemblyVersion(UpdateInstruction.MainAssembly);
            System.Version RollbackVersion = AssemblyManager.GetAssemblyVersion(UpdateBackupEntry.FullPath);

            updateResultReturn.AddMessage($"Trying to rollback version {CurrentVersion} to {RollbackVersion}", UpdateResultMessage.eMessageType.INFORMATION);

            var comandLineBeforeResult = Shell.ExecuteTerminalCommand(UpdateInstruction.CommandLineBeforeUpdate);
            if (comandLineBeforeResult.code > 0) throw new Exception($"Cannot run Commandline before update, see details: \r\n{comandLineBeforeResult.stdout}");
            updateResultReturn.AddMessage($"Command line 'CommandLineBeforeUpdate' executed.\r\nOutput code: {comandLineBeforeResult.code}\r\nOutput data:{comandLineBeforeResult.stdout}", UpdateResultMessage.eMessageType.SUCCESS);


            //Execuando Backup
            string workingFolderBackup = this.GenerateBackupFolderFullPathName(UpdateBackupEntry.UpdateID, updateResultReturn.ID, UpdateInstruction.WorkingDirectory, CurrentVersion);
            DirectoryManager.RenameDirectory(UpdateInstruction.WorkingDirectory, workingFolderBackup);

            updateResultReturn.AddMessage($"Renamed current version folder from {UpdateInstruction.WorkingDirectory} to {workingFolderBackup}.", UpdateResultMessage.eMessageType.SUCCESS);
            //Fazendo Rollback da versão anterior para versão atual de trabalho.
            //string BackupFolder = GetBackupFolderFromUpdateID()
            DirectoryManager.RenameDirectory(UpdateBackupEntry.FullPath, UpdateInstruction.WorkingDirectory);
            updateResultReturn.AddMessage($"Rollback diretory {UpdateBackupEntry.FullPath} to {UpdateInstruction.WorkingDirectory}.", UpdateResultMessage.eMessageType.SUCCESS);


            var comandLineAfterResult = Shell.ExecuteTerminalCommand(UpdateInstruction.CommandLineAfterUpdate);
            if (comandLineAfterResult.code > 0) throw new Exception($"Cannot run Commandline after update, see details: \r\n{comandLineAfterResult.stdout}");
            updateResultReturn.AddMessage($"Command line 'CommandLineBeforeUpdate' executed.\r\nOutput code: {comandLineAfterResult.code}\r\nOutput data:{comandLineAfterResult.stdout}", UpdateResultMessage.eMessageType.SUCCESS);


            stopWatchTimer.Stop();
            updateResultReturn.TimeSpentMilliseconds = stopWatchTimer.ElapsedMilliseconds;

            string alepsedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        stopWatchTimer.Elapsed.Hours,
                        stopWatchTimer.Elapsed.Minutes,
                        stopWatchTimer.Elapsed.Seconds,
                        stopWatchTimer.Elapsed.Milliseconds);

            updateResultReturn
                .AddMessage($"Rollback '{UpdateInstruction.Name}' from '{CurrentVersion}' to '{RollbackVersion}'. Alepsed time: {alepsedTime}",
                            updateResultReturn.IsSuccess ? UpdateResultMessage.eMessageType.SUCCESS : UpdateResultMessage.eMessageType.ERROR);

            this._UpdateRepository.WriteUpdateInstructionResult(updateResultReturn);
            return updateResultReturn;
        }

        

        public List<UpdateInstruction> GetUpdateInstructionList()
        {
            return _UpdateRepository.GetUpdateInstructionList();
        }

        public UpdateInstruction GetUpdateInstructionByID(Guid id)
        {
            return _UpdateRepository.GetUpdateInstructionByID(id);
        }

        public List<UpdateResult> GetUpdateHistory(UpdateInstruction instruction)
        {
            return _UpdateRepository.GetUpdateHistory(instruction);
        }

        public List<UpdateBackupEntry> GetUpdateBackupEntryList(UpdateInstruction UpdateInstruction)
        {
            return _UpdateRepository.GetUpdateBackupEntryList(UpdateInstruction);
        }


        private string GenerateBackupFolderFullPathName(Guid UpdateIntructionID, Guid UpdateID, string WorkDirectory, Version version)
        {
            string BackupSufix = $"{UpdateIntructionID}_{UpdateID}_{version}_{DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss")}";

            return  $"{WorkDirectory}_{BackupSufix}";
        }
        
    }
}
