using System;
using CoreLogic.Structures;
using CoreLogic.Vector;
using CoreLogic.FileHandler;
using CoreLogic.FileHandler.String;
using CoreLogic.FileHandler.GenerateData;

namespace TestBackend
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var salesVector = new Vector<Sales>(1);

            string test1 = "EL-12345;15;Еблан;12.01.2006";
            Sales sales_test1 = StringHandler.StringToSales(test1);

            salesVector.Add(sales_test1);

            Console.WriteLine(salesVector[0]);

            var goodsVector = new Vector<Goods>(1);

            string test2 = "EL-12345;Коксакер;49,99";
            Goods goods_test2 = StringHandler.StringToGoods(test2);

            goodsVector.Add(goods_test2);

            Console.WriteLine(goodsVector[0]);

            Vector<Sales> salesFromFile = FileHandler.ReadSalesFromFile("TestSales.txt");

            Vector<Sales> generatedSalesFromFile = GenerateData.GenerateSales(15, salesFromFile);

            FileHandler.SaveSalesToFile("TestSalesSave.txt", generatedSalesFromFile);
        }
    }
}
