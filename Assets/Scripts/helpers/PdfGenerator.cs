using System.Collections;
using System.IO;
using sharpPDF;
using sharpPDF.Enumerators;
using UnityEngine;

public class PdfGenerator 
{
    public static IEnumerator CreatePDF()
    {
        string title = "TogStats.pdf"; // System.DateTime.Today.ToString("g");
        pdfDocument myDoc = new pdfDocument(title, "Me", false);
        pdfPage myFirstPage = myDoc.addPage();

        myFirstPage.addText("Trails of Gold", 10, 730, predefinedFont.csHelveticaOblique, 30, new pdfColor(predefinedColor.csOrange));



        /*Table's creation*/
        pdfTable myTable = new pdfTable();
        //Set table's border
        myTable.borderSize = 1;
        myTable.borderColor = new pdfColor(predefinedColor.csDarkBlue);

        /*Add Columns to a grid*/
        myTable.tableHeader.addColumn(new pdfTableColumn("Nom", predefinedAlignment.csRight, 120));
        myTable.tableHeader.addColumn(new pdfTableColumn("Titre", predefinedAlignment.csCenter, 120));
        myTable.tableHeader.addColumn(new pdfTableColumn("Or", predefinedAlignment.csLeft, 150));
        myTable.tableHeader.addColumn(new pdfTableColumn("Obstacles", predefinedAlignment.csLeft, 150));


        pdfTableRow myRow = myTable.createRow();
        myRow[0].columnValue = "Katarina";
        myRow[1].columnValue = "One shot, one kill";
        myRow[2].columnValue = "200";
        myRow[3].columnValue = "14";

        myTable.addRow(myRow);

        pdfTableRow myRow1 = myTable.createRow();
        myRow1[0].columnValue = "Sergei";
        myRow1[1].columnValue = "Turret killer";
        myRow1[2].columnValue = "160";
        myRow1[3].columnValue = "21";

        myTable.addRow(myRow1);

        Sprite sprite = Resources.Load<Sprite>("Pdf/PdfTitle");
        byte[] tex = sprite.texture.EncodeToJPG();
        //byte[] tex = sprite.texture.GetRawTextureData();
        myFirstPage.addImage(tex, 400, 600, 128, 128);

        /*Set Header's Style*/
        myTable.tableHeaderStyle = new pdfTableRowStyle(predefinedFont.csCourierBoldOblique, 12, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csLightOrange));
        /*Set Row's Style*/
        myTable.rowStyle = new pdfTableRowStyle(predefinedFont.csCourier, 8, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csWhite));
        /*Set Alternate Row's Style*/
        myTable.alternateRowStyle = new pdfTableRowStyle(predefinedFont.csCourier, 8, new pdfColor(predefinedColor.csBlack), new pdfColor(predefinedColor.csLightYellow));
        /*Set Cellpadding*/
        myTable.cellpadding = 10;
        /*Put the table on the page object*/
        myFirstPage.addTable(myTable, 5, 700);


        //byte[] tex = sprite.texture.GetRawTextureData();
        myFirstPage.addImage(tex, 0, 600, 128, 128);




        yield return null;

        myDoc.createPDF(title);
    }


    public static void _CreatePDFwithScreenShoot(byte[] screenshoot, int screenWidth, int screenHeight, string sPath,string gavetitle, string spriteTitle)
    {
        //string title = "TogStats.pdf";
        string title = gavetitle + ".pdf"; // System.DateTime.Today.ToString("g");
        pdfDocument myDoc = new pdfDocument(title, "La Suite", false);
        pdfPage myFirstPage = myDoc.addPage(screenHeight, screenWidth);

        myFirstPage.addImage(screenshoot, 0, 0, screenHeight, screenWidth);

        if (spriteTitle != null)
        {
            Sprite sprite = Resources.Load<Sprite>(spriteTitle);
            if (sprite != null)
            {
                byte[] tex = sprite.texture.EncodeToJPG();
                //byte[] tex = sprite.texture.GetRawTextureData();
                myFirstPage.addImage(tex, myFirstPage.width - 128 - 20, 20, 128, 128);
            }
        }

        myDoc.createPDF(sPath + "/" + title);
        //        myDoc.createPDF(title);
    }

    public static void CreatePDFwithScreenShoot(byte[] screenshoot, int screenWidth, int screenHeight, string spriteTitle)
    {
        string sDir = System.DateTime.Today.ToString("d").Replace('/', '_').Replace('\\', '_'); ;
        string sPath = Application.dataPath + "/../PDFFiles/" + sDir;
        
        if (!Directory.Exists(sPath))
        {
            Directory.CreateDirectory(sPath);
        }

        Debug.Log("path : " + sPath);
        string date = System.DateTime.Now.Hour.ToString("00") + "_" + System.DateTime.Now.Minute.ToString("00");
        _CreatePDFwithScreenShoot(screenshoot, screenWidth, screenHeight, sPath, date, spriteTitle);
    }
}
