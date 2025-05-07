using System;

namespace star
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the radius: ");
            int radius = int.Parse(Console.ReadLine());
            int size = 2 * (radius + 1);

            char[,] matrix = new char[size, 2 * size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 2 * size; j++)
                {
                    matrix[i, j] = ' ';
                }
            }

            for (int i = 2; i < size - 1; i++)
            {
                matrix[1, i] = '*';
                matrix[size - 1, i] = '*';
                matrix[i, 1] = '*';
            }
            for (int i = 0; i < size; i++)
            {
                matrix[i, size / 3 + size] = '*';
                matrix[i, 2 * size / 3 + size] = '*';
                matrix[size / 3, i + size] = '*';
                matrix[2 * size / 3, i + size] = '*';
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < 2 * size; j++)
                {
                    Console.Write(matrix[i, j]);
                }
                Console.WriteLine();
            }
        }

        // calculate the distance between (x1, y1) and (x2, y2)
        static double SqrDistance2D(double x1, double y1, double x2, double y2)
        {
            return Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);
        }

        // Checks if two values a and b are within a given distance.
        // |a - b| < distance
        static bool IsClose(double a, double b, double distance)
        {
            return Math.Abs(a - b) < distance;
        }
    }
}


/* example output
Enter the radius: 
>> 5
                *   *   
  *********     *   *   
 *              *   *   
 *              *   *   
 *          ************
 *              *   *   
 *              *   *   
 *              *   *   
 *          ************
 *              *   *   
 *              *   *   
  *********     *   *   

*/

/* example output (CHALLANGE)
Enter the radius: 
>> 5
                *   *  
      *         *   *  
   *     *      *   *  
  *                    
           ************
               *   *   
 *             *   *   
               *   *   
           ************
  *                    
   *     *    *   *    
      *       *   *    

*/