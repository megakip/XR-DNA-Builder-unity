//MIT License
//Copyright (c) 2023 DA LAB (https://www.youtube.com/@DA-LAB)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class Map : MonoBehaviour
{
    [Header("Google Maps API Key")]
    [Tooltip("Voer hier je Google Static Maps API key in.")]
    [SerializeField] // Maak dit veld zichtbaar in de Inspector
    private string apiKey = ""; // Hernoemd en standaard leeg gemaakt

    [Header("Map Settings")]
    public float lat = 53.30046f;
    public float lon = 6.398401f;
    public int zoom = 20;
    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.high;
    public enum type { roadmap, satellite, gybrid, terrain };
    public type mapType = type.satellite;
    private string url = "";
    private int mapWidth = 640;
    private int mapHeight = 640;
    // private bool mapIsLoading = false; //not used. Can be used to know that the map is loading 
    private Rect rect;

    private float latLast = 53.30046f;
    private float lonLast = 6.398401f;
    private int zoomLast = 20;
    private resolution mapResolutionLast = resolution.high;
    private type mapTypeLast = type.satellite;
    private bool updateMap = true;
    
    // Add opacity property
    [Range(0, 1)]
    public float opacity = 0.0f;
    private float opacityLast = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Controleer of de API key is ingevuld
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Google Maps API Key is niet ingesteld in de Inspector voor het Map-script!");
            // Optioneel: voorkom dat de kaart wordt geladen zonder sleutel
             updateMap = false; // Zet updateMap op false om te voorkomen dat GetGoogleMap wordt aangeroepen zonder key
             return; // Stop de Start methode hier
        }

        StartCoroutine(GetGoogleMap());
        rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
        
        // Apply initial opacity
        SetOpacity(opacity);
    }

    void Awake()
    {
        // Zorg dat de RawImage alpha waarde direct op 0 wordt gezet
        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            Color imageColor = rawImage.color;
            imageColor.a = opacity;
            rawImage.color = imageColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (updateMap && (!Mathf.Approximately(latLast, lat) || !Mathf.Approximately(lonLast, lon) || zoomLast != zoom || mapResolutionLast != mapResolution || mapTypeLast != mapType))
        {
            // Controleer opnieuw of de API key aanwezig is voor het geval deze tijdens runtime wordt gewist
            if (string.IsNullOrEmpty(apiKey))
            {
                 Debug.LogError("Google Maps API Key is leeg. Kan kaart niet updaten.");
                 updateMap = false; // Stop updates als de key weg is
                 return;
            }
            rect = gameObject.GetComponent<RawImage>().rectTransform.rect;
            mapWidth = (int)Math.Round(rect.width);
            mapHeight = (int)Math.Round(rect.height);
            StartCoroutine(GetGoogleMap());
            updateMap = false;
        }
        
        // Check if opacity has changed
        if (!Mathf.Approximately(opacityLast, opacity))
        {
            SetOpacity(opacity);
            opacityLast = opacity;
        }
    }

    /// <summary>
    /// Sets the opacity of the map image
    /// </summary>
    /// <param name="value">Opacity value between 0 (transparent) and 1 (opaque)</param>
    public void SetOpacity(float value)
    {
        opacity = Mathf.Clamp01(value);
        RawImage rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            Color imageColor = rawImage.color;
            imageColor.a = opacity;
            rawImage.color = imageColor;
            Debug.Log($"Map opacity set to {opacity}");
        }
    }

    IEnumerator GetGoogleMap()
    {
        // Controleer of de API key geldig is voordat de request wordt gemaakt
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Kan Google Map niet ophalen: API Key ontbreekt.");
            yield break; // Stop de coroutine
        }

        url = "https://maps.googleapis.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=" + zoom + "&size=" + mapWidth + "x" + mapHeight + "&scale=" + mapResolution + "&maptype=" + mapType + "&key=" + apiKey; // Gebruik de Inspector apiKey
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Google Maps API Error: " + www.error);
            Debug.LogError("URL used: " + url);
        }
        else
        {
            gameObject.GetComponent<RawImage>().texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            latLast = lat;
            lonLast = lon;
            zoomLast = zoom;
            mapResolutionLast = mapResolution;
            mapTypeLast = mapType;
            updateMap = true;
            
            // Re-apply opacity after loading new texture
            SetOpacity(opacity);
        }
    }

    /// <summary>
    /// Herlaad de kaart met de huidige instellingen
    /// </summary>
    [ContextMenu("Refresh Map")]
    public void RefreshMap()
    {
        // Force update door de lat iets te wijzigen en terug te zetten
        float originalLat = lat;
        lat += 0.0001f;
        lat = originalLat;
        
        StartCoroutine(GetGoogleMap());
    }
}