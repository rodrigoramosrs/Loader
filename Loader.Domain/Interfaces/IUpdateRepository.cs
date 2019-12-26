using Loader.Domain.Models;
using Loader.Domain.Models.Update;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loader.Domain.Interfaces
{
    public interface IUpdateRepository
    {
        List<UpdateInstruction> GetUpdateInstructionList();

        UpdateInstruction GetUpdateInstructionByID(Guid id);

        List<UpdateResult> GetUpdateHistory(UpdateInstruction instruction);

        List<UpdateBackupEntry> GetUpdateBackupEntryList(UpdateInstruction UpdateInstruction);

        UpdateBackupEntry GetUpdateBackupEntryFromUpdateID(UpdateInstruction UpdateInstruction, Guid rollbackUpdateID);

        bool WriteUpdateInstructionResult(UpdateResult updateResult);

        string GetBackupFolderFromUpdateID(string ID);


    }
}
