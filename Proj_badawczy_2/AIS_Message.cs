using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_badawczy
{

    static class Msg
    {
        public static readonly int WRONG_IMPORT_PATH = -100;
        public static readonly int WRONG_EXPORT_PATH = -101;
        public static readonly int CORRECT = 1000;
        public static readonly int ERROR = -1000;
    }


    static class C {

        public static readonly float MAX_SPEED = 30.0f;
        public static readonly float MAX_RANGE = 200.0f;
    }


    static class Path {
        public static readonly string RESULTS =
            "D:/Dokumenty/Studia/03 II stopnia/II semestr/Projekt_Badawczy_II/RESULTS2";
        public static readonly string DAYS =
            "D:/Dokumenty/Studia/03 II stopnia/II semestr/Projekt_Badawczy_II/RESULTS2/DAYS";
        public static readonly string SHIPS =
            "D:/Dokumenty/Studia/03 II stopnia/II semestr/Projekt_Badawczy_II/RESULTS2/SHIPS";
    }


    struct AIS_Message_Short
    {
        public string MMSI;
        public string BaseDateTime;
        public string LAT;
        public string LON;
        public string Range;
    }


    struct Position
    {
        public DateTime DateTime;
        public float LON;
        public float LAT;
        public float Range;
        private string _strLON;
        private string _strLAT;
        private string _strRange;
        private string _strDateTime;

        public string strDateTime {
            get {
                return DateTime.ToString();
            }
            set {
                _strDateTime = value.Replace(",", ".");
                Range = float.Parse(_strDateTime);
            }
        }

        public string strRange {
            get {
                return Range.ToString().Replace(",", ".");
            }
            set {
                _strRange = value.Replace(",", ".");
                Range = float.Parse(_strRange);
            }
        }

        public string strLON {
            get {
                return LON.ToString().Replace(",", ".");
            }
            set {
                _strLON = value.Replace(",", ".");
                LON = float.Parse(_strLON);
            }
        }

        public string strLAT {
            get {
                return LAT.ToString().Replace(",", ".");
            }
            set {
                _strLAT = value.Replace(",", ".");
                LAT = float.Parse(_strLAT);
            }
        }



    }



    // klasa zawierająca poszczególne pola 
    // wiadomości odebranej
    class AIS_Message
    {
        public string MMSI;
        public string BaseDateTime;
        public string LAT;
        public string LON;
        public string SOG;
        public string COG;
        public string Heading;
        public string VesselName;
        public string IMO;
        public string CallSigh;
        public string VesselType;
        public string Status;
        public string Length;
        public string Width;
        public string Draft;
        public string Cargo;
        public string AISClass;
        public List<Position> Pos = new();
        public string Date {
            get {
                string str_data;
                if (this.Pos.Count > 0) {
                    DateTime data = this.Pos[0].DateTime;
                    str_data = data.Year.ToString() + "_" + data.Month.ToString() + "_" + data.Day.ToString() + "_data";
                }
                else str_data = "info";
                return str_data;
            }
        }

        void do_sth()
        {

        }

        public string wypisz_parametry()
        {
            string napis;
            napis = "\nMMSI: " + this.MMSI;
            napis += "\nLON: " + this.LON;
            napis += "\nVesselName: " + this.VesselName;
            napis += "\nIMO: " + this.IMO;
            napis += "\nCallSign: " + this.CallSigh;
            napis += "\nVesselType: " + this.VesselType;
            napis += "\nLength: " + this.Length;
            napis += "\nWidth: " + this.Width;
            napis += "\nDraft: " + this.Draft;
            napis += "\nAISClass: " + this.AISClass;
            napis += "\n";


            return napis;
        }

        public void wypisz_dane() {

            for (int i = 0; i < this.Pos.Count; i++) {

                Console.WriteLine(this.Pos[i].DateTime);
            }
        }

        public void filter_data() {

            TimeSpan differ_1;
            TimeSpan differ_2;
            // ^ różnica czasu pomiędzy kolejnymi pomiarami
            float distance;
            float distance_1;
            float distance_2;
            // ^ odległość między kolejnymi pozycjami statku
            float differ_hours;
            float differ_1_hours;
            float differ_2_hours;
                // ^ różnica czasu pomiędzy pomiarami w godzinach
            int i = 1;
                // ^ wskazuje aktualnie sprawdzaną pozycję
            int end = this.Pos.Count;
                // ^ liczba elementów listy zawierającej pozycje


            if (end < 3) {
               /* jeżeli posiadamy zbyt mało danych do przeprowadzenia
                * filtracji od danego statku
               */

                this.Pos.Clear();
                    // ^ nie będziemy brali tego obiektu pod uwagę

                return;
            }


            distance_1 = Encoder.Spheral_distance(Pos[0].LAT, Pos[0].LON, Pos[1].LAT, Pos[1].LON);
            distance_2 = Encoder.Spheral_distance(Pos[1].LAT, Pos[1].LON, Pos[2].LAT, Pos[2].LON);

            differ_1 = Pos[1].DateTime - Pos[0].DateTime;
            differ_2 = Pos[2].DateTime - Pos[1].DateTime;

            differ_1_hours = (float)differ_1.TotalHours;
            differ_2_hours = (float)differ_2.TotalHours;

            distance_1 /= Math.Abs(differ_1_hours);
            distance_2 /= Math.Abs(differ_2_hours);

            if ((distance_1 > C.MAX_SPEED && distance_2 < C.MAX_SPEED)) {
                // ^ jeżeli pierwszy wynik jest błędny

                this.Pos.RemoveAt(0);
                // ^ usuwamy pierwszy rekord danych
                end--;
            }


            while (i < end) {
                /// Console.WriteLine("i: {0} \tend: {1}, \tPos: {2}", i, end, this.Pos.Count);
                distance = Encoder.Spheral_distance(Pos[i - 1].LAT, Pos[i - 1].LON, Pos[i].LAT, Pos[i].LON);
                differ_hours = Math.Abs( (float) (Pos[i].DateTime - Pos[i - 1].DateTime).TotalHours );

                if (differ_hours > 0.0f)    distance /= differ_hours; 
                else                        distance = C.MAX_SPEED;

                if (distance > C.MAX_SPEED) {

                    Pos.RemoveAt(i);
                    end--;
                    
                }
                else i++;

                
            }

            i = 0;
            // ^ wskazuje aktualnie sprawdzaną pozycję
            end = this.Pos.Count;
            // ^ liczba elementów listy zawierającej pozycje

            while (i < end) {

                if (this.Pos[i].Range > C.MAX_RANGE) {

                    this.Pos.RemoveAt(i);
                    end--;

                }
                else i++;

            }


        }
    }
}
