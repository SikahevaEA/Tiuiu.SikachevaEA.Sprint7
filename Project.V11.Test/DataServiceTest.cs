using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Project.V11.Lib;

namespace Project.V11.Test
{
    [TestClass]
    public class DataServiceTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            
            
            string testFilePath = @"C:\Users\lizas\source\repos\Tiuiu.SikachevaEA.Sprint7\Project.V11\bin\Debug\DataBase.Project.csv";

            int lineCount = 0;

            
            using (var reader = new StreamReader(testFilePath))
            {
                // Пропускаем заголовок
                reader.ReadLine();

                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            
            Assert.AreEqual(14, lineCount); // Предполагаемое количество строк: 14
        }
    }
}
