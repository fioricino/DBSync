using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Database.Interfaces;
using DBSyncNew.Graph;

namespace DBSyncNew.SchemaObjects
{
    public class ForeignKeyObj : EdgeSource<AliasObj>, IReversable<ForeignKeyObj>
    {
        //TODO remove
        public ForeignKeyObj()
        {
            
        }

        private ForeignKeyObj(DBBuilder builder)
        {
            Column = builder.DBForeignKey.Column;
            ReferencedColumn = builder.DBForeignKey.ReferencedColumn;
            Alias = builder.Alias;
            ReferencedAlias = builder.ReferencedAlias;
            Direction = builder.Direction;
        }

        private ForeignKeyObj(ConfigBuilder builder)
        {
            Column = builder.ForeignKeyConfig.Column;
            ReferencedColumn = builder.ForeignKeyConfig.ReferencedColumn;
            Alias = builder.Alias;
            ReferencedAlias = builder.ReferencedAlias;
            Direction = builder.Direction;
        }

        private ForeignKeyObj(ForeignKeyObj source, bool reverse)
        {
            Alias = reverse ? source.ReferencedAlias : source.Alias;
            ReferencedAlias = reverse ? source.Alias : source.ReferencedAlias;
            Column = reverse ? source.ReferencedColumn : source.Column;
            ReferencedColumn = reverse ? source.Column : source.ReferencedColumn;
            Direction = source.Direction;
        }

        public string Column { get; }

        public string ReferencedColumn { get; }

        //TODO remove
        public AliasObj Alias { get; }

        //TODO remove
        public AliasObj ReferencedAlias { get; }

        public override AliasObj Start => Alias;
        public override AliasObj End => ReferencedAlias;
        public override ForeignKeyDirection Direction { get; set; }

        // TODO add ToString everywhere

            //TODO maybe static methods?
            //TODO create inside builder?
        public class DBBuilder
        {
            public DBBuilder(IDBForeignKey dbForeignKey, ForeignKeyDirection direction, AliasObj alias, AliasObj referencedAlias)
            {
                DBForeignKey = dbForeignKey;
                Direction = direction;
                Alias = alias;
                ReferencedAlias = referencedAlias;
            }


            public IDBForeignKey DBForeignKey { get; }

            public ForeignKeyDirection Direction { get; }

            public AliasObj Alias { get; }

            public AliasObj ReferencedAlias { get; }

            public ForeignKeyObj Build()
            {
                return new ForeignKeyObj(this);
            }
        }

        //TODO join both
        public class ConfigBuilder
        {
            public ConfigBuilder(ForeignKeyConfig foreignKeyConfig, ForeignKeyDirection direction, AliasObj alias, AliasObj referencedAlias)
            {
                ForeignKeyConfig = foreignKeyConfig;
                Direction = direction;
                Alias = alias;
                ReferencedAlias = referencedAlias;
            }

            public ForeignKeyConfig ForeignKeyConfig { get; }

            public ForeignKeyDirection Direction { get; }

            public AliasObj Alias { get; }

            public AliasObj ReferencedAlias { get; }

            public ForeignKeyObj Build()
            {
                return new ForeignKeyObj(this);
            }
        }

        public ForeignKeyObj Reversed()
        {
            return new ForeignKeyObj(this, true);
        }
    }
}
