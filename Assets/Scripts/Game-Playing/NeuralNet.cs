using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class NeuralNet 
{
    float[][] layer;
    float[][,] weights;
    float[][] bias;
    int layerCount;
    public delegate float ActivationFunction(float val);
    ActivationFunction activation;

    public NeuralNet()
    {
        activation = (x) => Mathf.Clamp(x,0f,1f);
    }
    public NeuralNet(ActivationFunction type)
    {
        activation = type;
    }

    public bool CreateNetwork(params int[] neuronsPerlayer)
    {
        if(neuronsPerlayer.Length<2)
            return false;
        //intializing layers
        layerCount = neuronsPerlayer.Length;    
        layer = new float[neuronsPerlayer.Length][];
        for(int i=0;i<neuronsPerlayer.Length;i++)
        {
            layer[i] = new float[neuronsPerlayer[i]];
        }

        //intializing weights and biases
        weights = new float[neuronsPerlayer.Length-1][,];
        bias = new float[neuronsPerlayer.Length - 1][];
        for(int i=0;i<neuronsPerlayer.Length -1;i++)
        {
            weights[i] = new float[neuronsPerlayer[i+1],neuronsPerlayer[i]];
            bias[i] = new float[neuronsPerlayer[i+1]];
        }

        return true;    
    }

    public void IntializeWeightsAndbiases(Vector2 weightRange,Vector2 biasRange)
    {
        for(int i=0;i<weights.Length;i++)
            for(int j=0;j<weights[i].GetLength(0);j++)
                for(int k=0;k<weights[i].GetLength(1);k++)
                    weights[i][j,k] = Random.Range(weightRange.x,weightRange.y);

        for(int i=0;i<bias.Length;i++)
            for(int j=0;j<bias[i].Length;j++)
                bias[i][j] = Random.Range(biasRange.x,biasRange.y);            
    }
    // float ActivationFunction(float val)
    // {
    //     return Mathf.Clamp01(val);
    // }

    public float[] ForwardPropagation(float[] input)
    {
        for(int i=0;i<input.Length;i++)
            layer[0][i] = input[i];
        for(int i=0;i<layerCount-1;i++)
        {
            EvaluateNextLayer(i);
        }
        return layer[layerCount - 1];
    }

    public void EvaluateNextLayer(int layerIndex)
    {
        //weight[0] * layer[0] MM + bias;
        for(int i=0;i<weights[layerIndex].GetLength(0);i++)
        {
            float temp = 0f;
            for(int j=0;j<weights[layerIndex].GetLength(1);j++)
                temp+=weights[layerIndex][i,j] * layer[layerIndex][j];
            layer[layerIndex + 1][i] = activation(temp + bias[layerIndex][i]);    
        }
    }

    public bool SetWeights(int index,float[,] values)
    {
        if(index<layerCount && weights[index].GetLength(0)==values.GetLength(0)&&weights[index].GetLength(1)==values.GetLength(1))
        {
            for(int i=0;i<weights[index].GetLength(0);i++)
                for(int j=0;j<weights[index].GetLength(1);j++)
                    weights[index][i,j] = values[i,j];
            return true;
        }
        return false;    
    }        

    public bool SetBias(int index,float[] values)
    {
        if(index < layerCount && bias[index].Length==values.Length)
        {
            for(int i=0;i<bias[index].Length;i++)
                bias[index][i] = values[i];
            return true;
        }
        return false;
    }

    public bool SetWeights(float[][,] wt)
    {
        if(weights.Length==wt.Length)
        {
            for(int i=0;i<wt.Length;i++)
            {
                if(weights[i].GetLength(0)!=wt[i].GetLength(0)||weights[i].GetLength(1)!=wt[i].GetLength(1))
                {
                    return false;
                }
            }
        }
        else
            return false;
        for(int i=0;i<wt.Length;i++)
        { 
            for(int j=0;j<wt[i].GetLength(0);j++)
                for(int k=0;k<wt[i].GetLength(1);k++)
                    weights[i][j,k]=wt[i][j,k];
        }
        return true;
    }

    public bool SetBias(float [][] wt)
    {
        if(bias.Length==wt.Length)
        {
            for(int i=0;i<wt.Length;i++)
            {
                if(weights[i].Length!=wt[i].Length)
                {
                    return false;
                }
            }
        }
        else
            return false;
        for(int i=0;i<wt.Length;i++)
        { 
            for(int j=0;j<wt[i].Length;j++)
                bias[i][j]=wt[i][j];
        }
        return true;    
    }

    public float[,] GetWeights(int index)
    {
        float[,] temp = new float[weights[index].GetLength(0),weights[index].GetLength(1)];
        for(int i=0;i<weights[index].GetLength(0);i++)
                for(int j=0;j<weights[index].GetLength(1);j++)
                    temp[i,j] = weights[index][i,j];
        return temp;
    }

    public float[] GetBias(int index)
    {
        float[] temp = new float[bias[index].Length];
        for(int i=0;i<bias[index].Length;i++)
                temp[i] = bias[index][i];
        return temp;
    }

    public float[][,] GetWeights()
    {
        float[][,] temp = new float[weights.Length][,];
        for(int i=0;i<weights.Length;i++)
        {
            temp[i] = new float[weights[i].GetLength(0),weights[i].GetLength(1)];
            for(int j=0;j<weights[i].GetLength(0);j++)
                for(int k=0;k<weights[i].GetLength(1);k++)
                {
                    temp[i][j,k] = weights[i][j,k];    
                }
        }
        return temp;
    }

    public float[][] GetBias()
    {
        float[][] temp = new float[bias.Length][];
        for(int i=0;i<bias.Length;i++)
        {
            temp[i] = new float[bias[i].Length];
            for(int j=0;j<bias[i].Length;j++)
            {
                temp[i][j] = bias[i][j];    
            }
        }
        return temp;
    }

    public static bool CompareWeights(float[][,] wt1,float[][,] wt2)
    {
        for(int i=0;i<wt1.Length;i++)
        { 
            for(int j=0;j<wt1[i].GetLength(0);j++)
                for(int k=0;k<wt1[i].GetLength(1);k++)
                    if(!wt1[i][j,k].Equals(wt2[i][j,k]))
                    {
                        return false;      
                    }
        }
        return true;
    }

    public static bool CompareBias(float[][] wt1,float[][] wt2)
    {
        for(int i=0;i<wt1.Length;i++)
        { 
            for(int j=0;j<wt1[i].Length;j++)
                if(!wt1[i][j].Equals(wt2[i][j]))
                {
                    return false;
                }
        }
        return true;
    }

}
