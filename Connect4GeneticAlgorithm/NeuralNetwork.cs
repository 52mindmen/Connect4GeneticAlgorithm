using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class NeuralNetwork : Opponent
{
    Layer[] layers;

    public NeuralNetwork(params int[] layerSizes)
    {
        layers = new Layer[layerSizes.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i + 1]);
        }
    }

    public NeuralNetwork(double[][,] weights, double[][] biases, params int[] layerSizes)
    {
        layers = new Layer[layerSizes.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i + 1], weights[i], biases[i]);
        }
    }

    public double[] CalculateOutputs(double[] inputs)
    {
        foreach (Layer layer in layers)
        {
            inputs = layer.CalculateOutputs(inputs);
        }
        return inputs;
    }

    public int Classify(double[] inputs, List<int> indexes)
    {
        double[] outputs = CalculateOutputs(inputs);
        return IndexOfMaxValue(outputs, indexes);
    }

    private int IndexOfMaxValue(double[] outputs, List<int> indexes)
    {
        double max = double.MinValue;
        int maxIndex = 0;
        foreach(int i in indexes)
        {
            if (outputs[i] > max)
            {
                max = outputs[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    public Tuple<double[][,], double[][]> GetGenes()
    {
        double[][,] weights = new double[layers.Length][,];
        double[][] biases = new double[layers.Length][];
        int length = layers.Length;
        for (int i = 0; i < length; i++)
        {
            Layer layer = layers[i];
            weights[i] = layer.weights;
            biases[i] = layer.biases;
        }

        return new Tuple<double[][,], double[][]> ( weights, biases);
    }

    override public int GetMove(ConnectFour game, int player)
    {
        double[] board = new double[42];
        for (int i = 0; i < 42; i++)
        {
            board[i] = game.board[i] * player;
        }
        return this.Classify(board, game.availableMoves);
    }
}
