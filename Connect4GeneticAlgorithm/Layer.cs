using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Layer
{
    public int numNodesIn, numNodesOut;
    public double[,] weights;
    public double[] biases;

    public Layer(int numNodesIn, int numNodesOut)
    {
        this.numNodesIn = numNodesIn;
        this.numNodesOut = numNodesOut;

        weights = new double[numNodesIn,numNodesOut];
        biases = new double[numNodesOut];
        InitializeRandomWeightsAndBiases();
    }

    public Layer(int numNodesIn, int numNodesOut, double[,] weights, double[] biases)
    {
        this.numNodesIn = numNodesIn;
        this.numNodesOut = numNodesOut;
        this.weights = weights;
        this.biases = biases;
    }

    private void InitializeRandomWeightsAndBiases()
    {
        for (int nodeOut = 0; nodeOut < numNodesOut; nodeOut++)
        {
            double randomValue = ThreadSafeRandom.NextDouble() * 2 - 1;
            biases[nodeOut] = randomValue / Math.Sqrt(numNodesOut);
            for (int nodeIn = 0; nodeIn < numNodesIn; nodeIn++)
            {
                randomValue = ThreadSafeRandom.NextDouble() * 2 - 1;
                weights[nodeIn, nodeOut] = randomValue / Math.Sqrt(numNodesIn);
            }
        }
    }

    public double[] CalculateOutputs(double[] inputs)
    {
        double[] activations = new double[numNodesOut];

        for (int nodeOut = 0; nodeOut < numNodesOut; nodeOut++)
        {
            double weightedInput = biases[nodeOut];
            for (int nodeIn = 0; nodeIn < numNodesIn; nodeIn++)
            {
                weightedInput += inputs[nodeIn] * weights[nodeIn, nodeOut];
            }
            activations[nodeOut] = ActivationFunction(weightedInput);
        }
        return activations;
    }

    double ActivationFunction(double weightedInput)
    {
        //return 1 / (1 + Math.Exp(-weightedInput));
        //return Math.Max(weightedInput, 0); //ReLU
        return Math.Max(0.1 * weightedInput, weightedInput); //Leaky ReLU
    }
}
