﻿using Loader.Domain.Models;
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

        bool WriteUpdateInstructionResult(UpdateResult updateResult);

        string GetBackupFolderFromUpdateID(string ID);
    }
}