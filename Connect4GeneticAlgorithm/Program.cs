using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

static class Program
{
    const int startingPopulation = 1024;
    const int populationGroups = 4;
    static readonly int[] layerSizes = { 42, 128, 64, 32, 16, 7 };
    const double mutationChance = 0.001;

    static List<NeuralNetwork>[] population = new List<NeuralNetwork>[populationGroups];
    static void Main()
    {
        for (int i = 0; i < populationGroups; i++)
        {
            population[i] = new List<NeuralNetwork>();
            for (int j = 0; j < startingPopulation; j++)
            {
                population[i].Add(new NeuralNetwork(layerSizes));
            }
        }

        CompetitionBot competitionBot = new CompetitionBot();

        int gamesWon, gamesLost, ties, total;
        int lastGamesWon = 0, lastGamesLost = 0, lastTies = 0;
        int maxGamesWon = 0, minGamesLost = populationGroups * startingPopulation, maxTies = 0;
        Console.WriteLine("Starting!");
        int iteration = 0;
        while (true)
        {
            gamesWon = 0;
            gamesLost = 0;
            ties = 0;
            total = 0;
            for (int i = 0; i < populationGroups; i++)
            {
                for (int j = 0; j < startingPopulation; j++)
                {
                    switch (PlayGame(population[i][j], competitionBot))
                    {
                        case -1:
                            gamesLost++;
                            total++;
                            break;
                        case 0:
                            ties++;
                            total++;
                            break;
                        case 1:
                            gamesWon++;
                            total++;
                            break;
                        default: break;
                    }
                }
            }

            maxGamesWon = Math.Max(maxGamesWon, gamesWon);
            minGamesLost = Math.Min(minGamesLost, gamesLost);
            maxTies = Math.Max(maxTies, ties);

            Console.WriteLine("Versus Competitive Bot");
            Console.WriteLine($"Current / Max Games  Won: {gamesWon} ({(gamesWon < lastGamesWon ? "-" : "+")}{Math.Abs(gamesWon - lastGamesWon)}) / {maxGamesWon}");
            Console.WriteLine($"Current / Min Games Lost: {gamesLost} ({(gamesLost < lastGamesLost ? "-" : "+")}{Math.Abs(gamesLost - lastGamesLost)}) / {minGamesLost}");
            Console.WriteLine($"Current / Max Games Tied: {ties} ({(ties < lastTies ? "-" : "+")}{Math.Abs(ties - lastTies)}) / {maxTies}");
            Console.WriteLine();

            Console.WriteLine($"Iteration {iteration++}");
            DateTime start = DateTime.Now;
            using (var progress = new ProgressBar())
            {
                for (int i = 0; i < populationGroups; i++)
                {
                    List<NeuralNetwork> populationGroup = population[i];


                    for (int j = 0; j < 100; j++)
                    {
                        progress.Report((double)(j + (i * 100)) / (populationGroups * 100));

                        List<NeuralNetwork> sortedNeuralNetworks;
                        sortedNeuralNetworks = Sort(populationGroup);
                        List<NeuralNetwork> bredNeuralNetworks;
                        bredNeuralNetworks = Breed(sortedNeuralNetworks);
                        population[i] = bredNeuralNetworks;
                    }
                }
            }
            DateTime end = DateTime.Now;
            Console.WriteLine($"Time Elapsed: {end - start}");
            List<NeuralNetwork>[] swappedPopulation = SwapPopulation(population);
            population = swappedPopulation;

            lastGamesWon = gamesWon;
            lastGamesLost = gamesLost;
            lastTies = ties;
        }
    }

    static List<NeuralNetwork>[] SwapPopulation(List<NeuralNetwork>[] populationToSwap)
    {
        ConcurrentBag<NeuralNetwork>[] swappedPopulation = new ConcurrentBag<NeuralNetwork>[populationGroups];
        List<int> numbers = new List<int>();

        for (int i = 0; i < populationGroups; i++)
        {
            swappedPopulation[i] = new ConcurrentBag<NeuralNetwork>();
            numbers.Add(i);
        }

        for (int i = 0; i < startingPopulation; i++)
        {
            for (int j = (numbers.Count - 1); j > 0; j--)
            {
                int k = ThreadSafeRandom.Next(j);
                (numbers[k], numbers[j]) = (numbers[j], numbers[k]);
            }
            Parallel.For(0, populationGroups, j => {
                swappedPopulation[j].Add(populationToSwap[numbers[j]][i]);
            });
        }

        List<NeuralNetwork>[] result = new List<NeuralNetwork>[populationGroups];
        for (int i = 0; i < populationGroups; i++)
        {
            result[i] = swappedPopulation[i].ToList();
        }

        return result;
    }

    static List<NeuralNetwork> Breed(List<NeuralNetwork> population)
    {
        ConcurrentBag<NeuralNetwork> children = new ConcurrentBag<NeuralNetwork>();

        int elitistSurvivorship = startingPopulation / 8; //bring top 1/8 into next generation
        int childrenCount = startingPopulation - elitistSurvivorship;

        int halfPopulation = startingPopulation / 2;

        for (int i = 0; i < elitistSurvivorship; i++)
        {
            children.Add(population[i]);
        }

        Parallel.For(0, childrenCount, i =>
        {
            int r1 = ThreadSafeRandom.Next(halfPopulation);
            int r2 = ThreadSafeRandom.Next(halfPopulation);

            if (r1 == r2)
            {
                children.Add(population[r1]);
            }
            else
            {
                children.Add(Breed(population[r1], population[r2], mutationChance));
            }
        });

        return children.ToList();
    }

    static NeuralNetwork Breed(NeuralNetwork parentA, NeuralNetwork parentB, double mutationChance)
    {
        (double[][,] weightsA, double[][] biasesA) = parentA.GetGenes();
        (double[][,] weightsB, double[][] biasesB) = parentB.GetGenes();

        int totalLayers = layerSizes.Length - 1;
        double[][,] childWeights = new double[totalLayers][,];
        double[][] childBiases = new double[totalLayers][];

        for (int i = 0; i < totalLayers; i++)
        {
            int weightsLengthDimension1 = layerSizes[i];
            int weightsLengthDimension2 = layerSizes[i + 1];
            int biasesLength = layerSizes[i + 1];

            childBiases[i] = new double[biasesLength];
            childWeights[i] = new double[weightsLengthDimension1, weightsLengthDimension2];

            for (int j = 0; j < weightsLengthDimension2; j++)
            {
                double randomValue = ThreadSafeRandom.NextDouble();
                if (randomValue < mutationChance)
                {
                    childBiases[i][j] = ThreadSafeRandom.NextDouble() * 2 - 1;
                }
                else
                {
                    childBiases[i][j] = FlipCoin() ? biasesA[i][j] : biasesB[i][j];
                }

                for (int k = 0; k < weightsLengthDimension1; k++)
                {
                    randomValue = ThreadSafeRandom.NextDouble();
                    if (randomValue < mutationChance)
                    {
                        childWeights[i][k, j] = ThreadSafeRandom.NextDouble() * 2 - 1;
                    }
                    else
                    {
                        childWeights[i][k, j] = FlipCoin() ? weightsA[i][k, j] : weightsB[i][k, j];
                    }
                }
            }

        }

        NeuralNetwork child = new NeuralNetwork(childWeights, childBiases, layerSizes);
        return child;
    }

    /*    static void WriteToFile(string text)
        {
            const string path = @"C:\Users\Aaron\source\repos\Connect4GeneticAlgorithm\Connect4GeneticAlgorithm\winningString.txt";

            File.WriteAllText(path, text);
        }*/

    static int PlayGame(Opponent playerOne, Opponent playerTwo)
    {
        ConnectFour game = new ConnectFour();
        int move = playerOne.GetMove(game, 1);
        game.MakeMove(move);
        Status result;
        while ((result = game.CheckWin(move, game.CurrentPlayer == 1 ? -1 : 1)) == Status.Ongoing)
        {
            if (game.CurrentPlayer == 1)
            {
                move = playerOne.GetMove(game, 1);
                game.MakeMove(move);
            }
            else
            {
                move = playerTwo.GetMove(game, -1);
                game.MakeMove(move);
            }
        }

        switch (result)
        {
            case Status.P1Win:
                return 1;
            case Status.P2Win:
                return -1;
            case Status.Draw:
                return 0;
            case Status.Ongoing:
            default:
                throw new NotImplementedException();
        }
    }

    static void DisplayBothBoards(ConnectFour game)
    {
        string p1BoardString = Convert.ToString((long)game.P1Board, 2).PadLeft(64, '0');
        string p2BoardString = Convert.ToString((long)game.P2Board, 2).PadLeft(64, '0');

        p1BoardString = p1BoardString.Substring(22, 42);
        p2BoardString = p2BoardString.Substring(22, 42);

        for (int i = 0; i < 6; i++)
        {
            Console.WriteLine(p1BoardString.Substring(i * 7, 7));
        }
        Console.WriteLine();
        for (int i = 0; i < 6; i++)
        {
            Console.WriteLine(p2BoardString.Substring(i * 7, 7));
        }
        Console.WriteLine();
    }

    static bool FlipCoin()
    {
        return ThreadSafeRandom.NextDouble() >= 1.00 / 2.00;
    }

    static List<NeuralNetwork> Sort(List<NeuralNetwork> input)
    {
        int[] wins = new int[input.Count];

        const int iterations = 4;

        Parallel.For(0, input.Count, i =>
        {
            for (int j = 0; j < iterations; j++)
            {
                int r;
                while ((r = ThreadSafeRandom.Next(input.Count)) == i) { }
                wins[i] += PlayGame(input[i], input[r]);
            }
        });

        return input.Zip(wins).OrderByDescending(x => x.Second).Select(x => x.First).ToList();
    }
}
