using System.Collections.Generic;
using UnityEngine;

public static class QueueExtensions
{
    // Extension method to shuffle the queue
    public static void Shuffle<T>(this Queue<T> queue)
    {
        // Convert the queue to a list
        List<T> list = new List<T>(queue);

        // Shuffle the list using Fisher-Yates algorithm
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1); // Get a random index
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        // Clear the original queue
        queue.Clear();

        // Enqueue the shuffled items back into the queue
        foreach (T item in list)
        {
            queue.Enqueue(item);
        }
    }
}