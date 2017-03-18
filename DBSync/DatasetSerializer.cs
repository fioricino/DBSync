using System;
using System.Collections;
using System.Data;
using System.IO;

namespace DBSync
{
    public static class DatasetSerializer
    {
        public static void SerializeDataSet<T>(Stream stream, T ds) where T : DataSet
        {
            using (var bw = new BinaryWriter(stream))
            {
                SerializeDataSet(bw, ds);
            }
        }

        public static byte[] SerializeDataSet<T>(T data) where T : DataSet
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                SerializeDataSet(ms, data);
                bytes = ms.ToArray();
            }
            return bytes;
        }


        public static void SerializeDataSet<T>(BinaryWriter bw, T ds) where T : DataSet
        {
            var tables = ds.Tables;
            var iTabCount = tables.Count;

            var bitsNull = new BitArray(iTabCount);
            var byteNull = new byte[(iTabCount + 7) / 8];

            for (int i = 0; i < iTabCount; i++)
            {
                if (tables[i].Rows.Count == 0)
                {
                    bitsNull.Set(i, true);
                }
            }
            bitsNull.CopyTo(byteNull, 0);

            bw.Write(byteNull);

            foreach (DataTable dt in tables)
            {
                SerializeDataTable(bw, dt);
            }
        }

        public static T DeserializeDataSet<T>(Stream stream) where T : DataSet, new()
        {
            var ds = new T();

            var tables = ds.Tables;
            var iTabCount = tables.Count;
            int iBitLenInBytes = (iTabCount + 7) / 8;
            var byteNull = new byte[iBitLenInBytes];
            var bitsNull = new BitArray(byteNull);

            using (var br = new BinaryReader(stream))
            {
                br.Read(byteNull, 0, iBitLenInBytes);
                bitsNull = new BitArray(byteNull);

                for (int i = 0; i < iTabCount; i++)
                {
                    if (!bitsNull.Get(i))
                    {
                        DeserializeTable(br, tables[i]);
                    }
                }
            }

            return ds;
        }

        public static T DeserializeDataSet<T>(byte[] data) where T : DataSet, new()
        {
            using (var ms = new MemoryStream(data))
            {
                return DeserializeDataSet<T>(ms);
            }
        }

        private static void SerializeDataTable(BinaryWriter bw, DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                return;
            }
            DataColumnCollection columns = dt.Columns;
            int iColCount = columns.Count;

            bw.Write(dt.Rows.Count);

            foreach(DataRow dr in dt.Rows)
            {
                var bitsNull = new BitArray(iColCount);
                var byteNull = new byte[(iColCount + 7) / 8];
 
                for(int i = 0; i < iColCount; i++)
                {
                    if(dr[i] == DBNull.Value)
                        bitsNull.Set(i, true);
                }
                bitsNull.CopyTo(byteNull, 0);

                bw.Write(byteNull);

                for(int i = 0; i < iColCount; i++)
                {
                    object data = dr[i];
                    if (data == DBNull.Value || columns[i].ReadOnly)
                    {
                        continue;
                    }

                    if (data is Boolean)
                    {
                        bw.Write((Boolean)data);
                    }
                    else if (data is Char)
                    {
                        bw.Write((Char)data);                            
                    }
                    else if (data is SByte)
                    {
                        bw.Write((SByte) data);
                    }
                    else if (data is Byte)
                    {
                        bw.Write((Byte) data);
                    }
                    else if (data is Int16)
                    {
                        bw.Write((Int16) data);
                    }
                    else if (data is UInt16)
                    {
                        bw.Write((UInt16) data);
                    }
                    else if (data is Int32)
                    {
                        bw.Write((Int32) data);
                    }
                    else if (data is UInt32)
                    {
                        bw.Write((UInt32) data);
                    }
                    else if (data is Int64)
                    {
                        bw.Write((Int64) data);
                    }
                    else if (data is UInt64)
                    {
                        bw.Write((UInt64) data);
                    }
                    else if (data is Single)
                    {
                        bw.Write((Single) data);
                    }
                    else if (data is Double)
                    {
                        bw.Write((Double) data);
                    }
                    else if (data is Decimal)
                    {
                        bw.Write((Decimal) data);
                    }
                    else if (data is DateTime)
                    {
                        bw.Write(((DateTime) (data)).Ticks);
                    }
                    else if (data is byte[])
                    {
                        bw.Write(Convert.ToBase64String(data as byte[]));
                    }
                    else
                    {
                        bw.Write(data.ToString());
                    }
                }
            }
        }

        private static void SerializeDataTable(Stream stream, DataTable dt)
        {
            using (var bw = new BinaryWriter(stream))
            {
                SerializeDataTable(bw, dt);
            }
        }

        private static DataTable DeserializeTable(BinaryReader br, DataTable dt)
        {
            dt.BeginLoadData();

            DataColumnCollection columns = dt.Columns;
            int iColCount = columns.Count;
                
            int iBitLenInBytes = (iColCount + 7) / 8;

            int counRows = br.ReadInt32();
            DataRowCollection rows = dt.Rows;

            for(int r = 0; r < counRows; r++)
            {
                var byteNull = new byte[iBitLenInBytes];
                var bitsNull = new BitArray(byteNull);
                DataRow dr = dt.NewRow();
                rows.Add(dr);
                dr.BeginEdit();

                    br.Read(byteNull, 0, iBitLenInBytes);
                    bitsNull = new BitArray(byteNull);

                    for(int i = 0; i < iColCount; i++)
                    {
                        if (columns[i].ReadOnly)
                        {
                            continue;
                        }

                        if(bitsNull.Get(i))
                        {
                            dr[i] = DBNull.Value;
                            continue;
                        }

                        var columnType = dt.Columns[i].DataType;

                        if (columnType == typeof(Boolean) || columnType == typeof(Boolean?))
                        {
                            dr[i] = br.ReadBoolean();
                        }
                        else if (columnType == typeof(Char) || columnType == typeof(Char?))
                        {
                            dr[i] = br.ReadChar();
                        }
                        else if (columnType == typeof(String))
                        {
                            dr[i] = br.ReadString();
                        }
                        else if (columnType == typeof(SByte) || columnType == typeof(SByte?))
                        {
                            dr[i] = br.ReadSByte();
                        }
                        else if (columnType == typeof(Byte) || columnType == typeof(Byte?))
                        {
                            dr[i] = br.ReadByte();
                        }
                        else if (columnType == typeof(Int16) || columnType == typeof(Int16?))
                        {
                            dr[i] = br.ReadInt16();
                        }
                        else if (columnType == typeof(UInt16) || columnType == typeof(UInt16?))
                        {
                            dr[i] = br.ReadUInt16();
                        }
                        else if (columnType == typeof(Int32) || columnType == typeof(Int32?))
                        {
                            dr[i] = br.ReadInt32();
                        }
                        else if (columnType == typeof(UInt32) || columnType == typeof(UInt32?))
                        {
                            dr[i] = br.ReadUInt32();
                        }
                        else if (columnType == typeof(Int64) || columnType == typeof(Int64?))
                        {
                            dr[i] = br.ReadInt64();
                        }
                        else if (columnType == typeof(UInt64) || columnType == typeof(UInt64?))
                        {
                            dr[i] = br.ReadUInt64();
                        }
                        else if (columnType == typeof(Single) || columnType == typeof(Single?))
                        {
                            dr[i] = br.ReadSingle();
                        }
                        else if (columnType == typeof(Double) || columnType == typeof(Double?))
                        {
                            dr[i] = br.ReadDouble();
                        }
                        else if (columnType == typeof(Decimal) || columnType == typeof(Decimal?))
                        {
                            dr[i] = br.ReadDecimal();
                        }
                        else if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))
                        {
                            dr[i] = new DateTime(br.ReadInt64());
                        }
                        else if (columnType == typeof(Guid) || columnType == typeof(Guid?))
                        {
                            dr[i] = Guid.Parse(br.ReadString());
                        }
                        else if (columnType == typeof(byte[]))
                        {
                            dr[i] = Convert.FromBase64String(br.ReadString());
                        }
                    }

                dr.EndEdit();
            }
            dt.EndLoadData();
            return dt;
        }

        private static DataTable DeserializeTable(Stream stream, DataTable dt)
        {
            using (var br = new BinaryReader(stream))
            {
                return DeserializeTable(br, dt);
            }
        }
    }
}




