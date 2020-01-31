﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicGaussianBlur : MonoBehaviour
{
    public Material blurMaterial; //create material from shader and attatch here
    public float sigmaMaximum = 15f;
    public float angularAccelerationThreshold = 40f;

    [Range(0, 10)]
    public int smoothness = 1;
    [Range(.5f,20)]
    public float sigma = 0.5f;
    float[] kernel;
    
    void Start(){
        //initialize to some random matrix
        kernel = new float[121];
        angularVelocity = new Vector3(0,0,0);
        lastRotation = new Vector3(0,0,0);
        StartCoroutine(CalculateAngularAcceleration());
    }

    Vector3 lastRotation;
    Vector3 lastAngularVelocity;
    Vector3 angularVelocity;
    Vector3 angularAcceleration;
   


    bool calculatingAcceleration = true;
    float checkTime = 0.1f;
    IEnumerator CalculateAngularAcceleration(){
        yield return null;
        lastRotation = transform.localEulerAngles;
        lastAngularVelocity = new Vector3(0,0,0);
        angularAcceleration = new Vector3(0,0,0);
        angularVelocity = new Vector3(0,0,0);

        while(calculatingAcceleration){
            yield return new WaitForSeconds(checkTime);

            angularVelocity = (transform.localEulerAngles - lastRotation) / checkTime;
            angularAcceleration = angularVelocity - lastAngularVelocity;


            lastAngularVelocity = angularVelocity;
            lastRotation = transform.localEulerAngles;
            
        }
        
        
        
    }

    public bool adjustBasedOnRotation = true;
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {

        
        //calculate sigma, standard deviation
        if(adjustBasedOnRotation){

            sigma = Mathf.Lerp(0.5f, sigmaMaximum, (angularAcceleration.magnitude - angularAccelerationThreshold) );
        }

        //float kernelSum = kernel.Sum();
        //kernel = kernel.Select(kernel=>kernel/kernelSum).ToArray();
        kernel = new float[121];
        //initialize to some 
        for(int x = 0; x<11; x++){
            for(int y = 0; y<11; y++){
                kernel[y*11 + x] = GaussianFunction(x-5.0f, y-5.0f,sigma); //update kernel
            
        }
        //calculate sum for later
        float kernelSum = 0;
        for(int i = 0; i<kernel.Length; i++){
            kernelSum+= kernel[i];
        }

        for(int i = 0; i<kernel.Length; i++){
            kernel[i] *= (1f/kernelSum);
        }
        
        //communicate kernel through skybox
        blurMaterial.SetFloatArray("_kernel",kernel);
        blurMaterial.SetFloat("_kernelSum", kernelSum);
        blurMaterial.GetFloatArray("_kernel");

        RenderTexture renderTexture = RenderTexture.GetTemporary(src.width, src.height);

        Graphics.Blit(src, renderTexture); //copies source texture to destination texture

        //apply the render texture as many iterations as specified
        for (int i = 0; i < smoothness; i++)
        {
            RenderTexture tempTexture = RenderTexture.GetTemporary(src.width, src.height); //creates a quick temporary texture for calculations
            Graphics.Blit(renderTexture, tempTexture, blurMaterial);
            RenderTexture.ReleaseTemporary(renderTexture); //releases the temporary texture we got from GetTemporary 
            renderTexture = tempTexture;
        }

        Graphics.Blit(renderTexture, dst);
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    float GaussianFunction(float x, float y, float sigma){
        float p1 = 1f/ ((2f*Mathf.PI) * Mathf.Pow(sigma,2f));
        float eExponent = -(Mathf.Pow(x,2) + Mathf.Pow(y,2)) / (2*Mathf.Pow(sigma,2));
        float answer = p1 * Mathf.Exp(eExponent);
        return answer;
    }
}
}