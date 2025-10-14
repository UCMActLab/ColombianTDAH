using UnityEngine;

public class HourMinSec
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }

    // PUBLIC
    // Constructoras
    public HourMinSec(int h, int m, int s)
    {
        Hours = h;
        Minutes = m;
        Seconds = s;
    }

    public HourMinSec()
    {
        Hours = 0;
        Minutes = 0;
        Seconds = 0;
    }

    public HourMinSec(string h)
    {
        Debug.Log("Hora Metida en Clase (texto): " + h);

        string[] hSplit = h.Split(' '); // Separo numero texto
        string htype = hSplit[1].ToLower(); // Paso a minusculas

        Hours = int.Parse(hSplit[0]);

        // Si es am
        if (hSplit[1][0] == 'a')
        {
            if (Hours == 12)
                Hours = 0;
        }
        // Si es pm
        else
        {
            if (Hours != 12)
                Hours += 12;
        }

        Debug.Log("Hora después del calculo a 24: " +  Hours);
    }

    // Sobrecarga operador +
    public static HourMinSec operator +(HourMinSec t1, HourMinSec t2)
    {
        HourMinSec time = new HourMinSec(t1.Hours + t2.Hours, t1.Minutes + t2.Minutes, t1.Seconds + t2.Seconds);

        if (time.Seconds >= 60)
        {
            time.Minutes += (time.Seconds / 60);
            time.Seconds = (time.Seconds % 60);
        }

        if (time.Minutes >= 60)
        {
            time.Hours += (time.Minutes / 60);
            time.Minutes = (time.Minutes % 60);
        }

        time.Hours = (time.Hours % 24);

        return time;
    }

    // Sobrecarga operador -
    public static HourMinSec operator -(HourMinSec t1, HourMinSec t2)
    {
        HourMinSec time = new HourMinSec(t1.Hours - t2.Hours, t1.Minutes - t2.Minutes, t1.Seconds - t2.Seconds);

        if (time.Seconds < 0)
        {
            time.Minutes -= ((-time.Seconds / 60) + 1);
            time.Seconds = (60 - (-time.Seconds % 60));
        }

        if (time.Minutes < 0)
        {
            time.Hours -= ((-time.Minutes / 60) + 1);
            time.Minutes = (60 - (-time.Minutes % 60));
        }

        time.Hours = 24 - (-time.Hours % 24);
        time.Hours %= 24;
        return time;
    }

    // Devuelve tiempo en texto mayus
    public string GetString()
    {
        string aux;
        if (Hours >= 12)
            aux = "PM";
        else
            aux = "AM";

        if (Hours == 24)
            aux = "AM";

        string t = Hours + ":" + Minutes + ":" + Seconds + " " + aux;

        return t;
    }

    // Devuelve horas y minutos en formato 12:00 AM/PM
    public string GetHMString()
    {
        string aux;
        if (Hours >= 12)
            aux = "PM";
        else
            aux = "AM";

        int hour = Hours % 12;

        if (hour == 0)
        {
            hour = 12;
        }

        string auxM = "";

        if (Minutes < 10)
            auxM = "0";

        string t = hour + ":" + auxM + Minutes + " " + aux;

        return t;
    }

    // Devuelve horas en formato 12 am/pm
    public string GetHString()
    {
        string aux;
        if (Hours >= 12)
            aux = "pm";
        else
            aux = "am";

        int hour = Hours % 12;

        if (hour == 0)
        {
            hour = 12;
        }

        return (hour + " " + aux);
    }


    // Devuelve el numero de horas que hay entre las dos horas, restando el  parametro con la hora de la clase
    public int GetHoursInBetween(int hMax)
    {
        if(Hours > hMax)
        {
            hMax += 24;
        }

        return hMax-Hours;
    }
}
