using System;
using System.Data.SQLite;
using System.IO;


namespace DTED_Loader_SQLite_Library
{
    public class DTED_Loader_Library
    {
        SQLiteConnection connection;
        string DB_Path = @"D:\SQLite\DTED.db";

        public DTED_Loader_Library(string DBPath)
        {
            this.DB_Path = DBPath;
            string ConStr = "data source=" + DB_Path;
            if (!File.Exists(DB_Path))
            {
                Console.WriteLine("資料庫檔案不存在");
            }
            else
            {
                connection = new SQLiteConnection(ConStr);
                connection.Open();
            }

        }

        public void Disposed()
        {
            connection.Close();
        }

        

        public float Get_Height(float Source_Lon, float Source_Lat)
        {
            string Table_Name = "N" + ((int)Source_Lat).ToString() + "_" + "E" + ((int)Source_Lon).ToString();
            long Data_Index = Calculate_Index(Source_Lat, Source_Lon);
            string sql = "SELECT Height FROM " + Table_Name + " WHERE \"Index\" == " + Data_Index.ToString();
            string result;
            using (SQLiteCommand cmd = new SQLiteCommand(connection))
            {

                cmd.CommandText = sql;

                using (SQLiteDataReader queryResult = cmd.ExecuteReader())
                {
                    /// read one row one time
                    queryResult.Read();
                    result = queryResult.GetValue(0).ToString();
                    /// change index to get different column
                }
            }

            return float.Parse(result);

        }


        private long Calculate_Index(double lantitude, double longitude)
        {
            int row, column, temp;

            //2018DTED版本Precision為10，直接使用DTED_Files中Precision不等於30的case
            if (longitude - (int)longitude == 1.0)
            {
                row = 3600;
            }
            else
            {
                temp = (int)longitude;
                longitude = (longitude - temp) * 60;
                temp = (int)longitude;
                row = temp * 60;
                longitude = (longitude - temp) * 60;
                temp = (int)longitude;
                row = row + temp;
            }

            if (lantitude - (int)lantitude == 1.0)
            {
                column = 3600;
            }
            else
            {
                temp = (int)lantitude;
                lantitude = (lantitude - temp) * 60;
                temp = (int)lantitude;
                column = temp * 60;
                lantitude = (lantitude - temp) * 60;
                temp = (int)lantitude;
                column = column + temp;
            }

            return row * 3601 + column + 1;

            //Precision恰為30的處理方式
            /*
            if (longitude - Longitude == 1.0)
            {
                row = 1200;
            }
            else
            {
                temp = (int)longitude;
                longitude = (longitude - temp) * 60;
                temp = (int)longitude;
                row = temp * 20;
                longitude = (longitude - temp) * 60;
                temp = (int)longitude;
                row = row + (temp / 3);
            }

            if (lantitude - Lantitude == 1.0)
            {
                column = 1200;
            }
            else
            {
                temp = (int)lantitude;
                lantitude = (lantitude - temp) * 60;
                temp = (int)lantitude;
                column = temp * 20;
                lantitude = (lantitude - temp) * 60;
                temp = (int)lantitude;
                column = column + (temp / 3);
            }
            */

        }

    }
}
