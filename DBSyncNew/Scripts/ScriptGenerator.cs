using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.SchemaObjects;

namespace DBSyncNew.Scripts
{
    public class ScriptGenerator
    {
        public void AppendJoin(StringBuilder sbuilder, ForeignKeyObj key)
        {
            sbuilder.Append(" JOIN ");
            sbuilder.Append(key.ReferencedAlias.NameWithAlias);
            sbuilder.Append(" ON ");
            sbuilder.Append(string.Format("{0}.{1} = {2}.{3}",
                key.Alias.NameOrAlias,
                key.Column,
                key.ReferencedAlias.NameOrAlias,
                key.ReferencedColumn));
            sbuilder.AppendLine();
        }

        public string GenerateInsertMetadataTableScript(IList<TableObj> levelInfo)
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < levelInfo.Count; index++)
            {
                TableObj tableLevelInfo = levelInfo[index];
                string insertCommandText = string.Format(
                    @"INSERT INTO [dbo].[SYS_TUEV_TABLES] ([ID], [TABLE_NAME], [SCOPE_TYPE], [LEVEL], [IS_DATE_FILTERED]) VALUES({0},'{1}',{2},{3},{4});",
                    index, tableLevelInfo.Name, (int) tableLevelInfo.Scope.ScopeType, tableLevelInfo.Level, 0);
                sb.AppendLine(insertCommandText);
            }
            return sb.ToString();
        }
    }
}
