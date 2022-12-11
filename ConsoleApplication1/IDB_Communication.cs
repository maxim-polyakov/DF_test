using Microsoft.Data.Analysis;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Bridge
{
    public interface IDB_Communication
    {
        void insert_to(string text, string tablename, string str, string agenda, string classification,
            string classtype);
        void insert_to(DataFrame df, string datatable, string schema);
        void insert_to(Int64 idx, string txt, string insert, string datatable);
        void insert_to(DataFrame df, string datatable);
        void insert_to(string str, string datatable);
        void insert_to(string lowertext);
        void insert_to(DataFrame df);
        DataFrame get_data(string select);
        bool checkcommands(string input_string);
    }
    public class DB_Communication : IDB_Communication
    {
        public void insert_to(string text, string tablename, string str, string agenda, string classification, string classtype)
        {

        }

        public void insert_to(Int64 idx, string txt, string insert, string datatable)
        {

        }

        public void insert_to(DataFrame df, string datatable, string schema)
        {

        }

        public void insert_to(DataFrame df, string datatable)
        {

        }

        public void insert_to(string str, string datatable)
        {

        }

        public void insert_to(DataFrame df)
        {

        }

        public void insert_to(string lowertext)
        {

        }

        public DataFrame get_data(string select)
        {
            var cs = "Host=localhost;Username=postgres;Password=postgres;Database=MisaMemory";
            var conn = new NpgsqlConnection(cs);
            conn.Open();
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = select;
            cmd.ExecuteNonQuery();
            NpgsqlDataReader dr = cmd.ExecuteReader();

            SentimentData sd = new SentimentData();
            sd.id = new PrimitiveDataFrameColumn<int>("id", 0);
            sd.text = new StringDataFrameColumn("text", 0);
            sd.agenda = new StringDataFrameColumn("agenda", 0);
            sd.agendaid = new Int32DataFrameColumn("agendaid", 0);
            sd.hi = new Int32DataFrameColumn("hi", 0);
            sd.business = new Int32DataFrameColumn("business", 0);
            sd.weather = new Int32DataFrameColumn("weather", 0);
            sd.thanks = new Int32DataFrameColumn("thanks", 0);
            sd.emotionid = new Int32DataFrameColumn("emotionid", 0);
            sd.trash = new Int32DataFrameColumn("trash", 0);

            int id = 1;
            while (dr.Read())
            {
                sd.id.Append(id);
                sd.text.Append(dr[0].ToString());
                sd.agenda.Append(dr[1].ToString());
                sd.agendaid.Append(Int32.Parse(dr[2].ToString()));
                sd.hi.Append(Int32.Parse(dr[3].ToString()));
                sd.business.Append(Int32.Parse(dr[4].ToString()));
                sd.weather.Append(Int32.Parse(dr[5].ToString()));
                sd.thanks.Append(Int32.Parse(dr[6].ToString()));
                sd.emotionid.Append(Int32.Parse(dr[7].ToString()));
                sd.trash.Append(Int32.Parse(dr[8].ToString()));
                id++;
            }

            DataFrame df = new DataFrame(sd.id, sd.text, sd.agenda, sd.agendaid, sd.hi, sd.business, sd.weather, sd.thanks, sd.emotionid, sd.trash);
            return df;
        }

        public bool checkcommands(string input_string)
        {
            var cs = "Host=localhost;Username=postgres;Password=postgres;Database=MisaMemory";
            var conn = new NpgsqlConnection(cs);
            conn.Open();
            var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT text FROM assistant_sets.commands";
            cmd.ExecuteNonQuery();
            NpgsqlDataReader dr = cmd.ExecuteReader();

            PrimitiveDataFrameColumn<int> idx = new PrimitiveDataFrameColumn<int>("id", 0);
            StringDataFrameColumn text = new StringDataFrameColumn("text", 0);

            int id = 1;
            while (dr.Read())
            {
                idx.Append(id);
                text.Append(dr[0].ToString());
                id++;
            }

            DataFrame df = new DataFrame(idx, text);
            Dictionary<int, string> txt = new Dictionary<int, string>();

            for (int i = 0; i < df["text"].Length; i++)
            {
                txt.Add(i, df["text"][i].ToString());
            }
            foreach (var txtvalue in txt.Values)
            {
                if (input_string.Contains(txtvalue))
                {
                    return true;
                }

            }
            return false;
        }
    }
}
