using System;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PageSusCuentasMinigame : MonoBehaviour
{
    [SerializeField]private GameObject FacturaPrefab;
    [SerializeField]private List<Button> valueButtons;
    [SerializeField]private List<Button> facturaButtons;
    [SerializeField]private List<string> empresas;
    [SerializeField]private List<Transform> PaperPositions;
    [SerializeField]private TextMeshProUGUI currentFacturaText;
    [SerializeField]private TextMeshProUGUI TimerText;
    [SerializeField]private float timer;
    [SerializeField]private float ExitTimer;
    [SerializeField] private List<ActionData_SO> Results;
    private List<FacturaMono> factura=new List<FacturaMono>();
    private List<TextMeshProUGUI> valuesLabel=new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> facturaLabel=new List<TextMeshProUGUI>();
    private int CurrentFactura=0;
    private int PagosCorrectos=0;
    private int TaskCompleted=0;
    private List<Pago> pagos=new List<Pago>();
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
        for (int i = 0; i < 3; i++)
        {
            int monto = Random.Range(5000, 999999);
            int montoCorrecto = Random.Range(0, 3);
            Pago pago = new Pago();
            for (int j = 0; j < 4; j++)
            {
                pago.montos.Add((int)(monto*Random.Range(0.7f, 1.3f)));
                if (pago.montos[j] > 999999)
                    pago.montos[j] = 999999;
            }
            List<string> empresasDisponibles = new List<string>(empresas);
            if (i > 0)
            {
                for (int k = 0; k < i; k++)
                {
                    empresasDisponibles.Remove(pagos[k].empresa);
                }
            }
            Debug.Log(empresasDisponibles.Count);
            pago.empresa=empresasDisponibles[Random.Range(0, empresasDisponibles.Count)];
            pago.montos[montoCorrecto] = monto;
            pago.montoCorrecto = montoCorrecto;
            pagos.Add(pago);
        }

        for (int i = 0; i < 5; i++)
        {
            Transform point = PaperPositions[Random.Range(0, PaperPositions.Count)];
            PaperPositions.Remove(point);
            factura.Add(Instantiate(FacturaPrefab, point.position, point.rotation).GetComponent<FacturaMono>());
            List<string> empresasDisponibles = new List<string>(empresas);
            factura[i].Empresa.text = empresasDisponibles[Random.Range(0, empresasDisponibles.Count)];
            factura[i].Valor.text = Random.Range(5000, 999999).ToString();
            if (i < pagos.Count)
            {
                factura[i].Empresa.text = pagos[i].empresa;
                factura[i].Valor.text = pagos[i].montos[pagos[i].montoCorrecto].ToString();
            }
            if (i > 0)
            {
                for (int k = 0; k < i; k++)
                {
                    empresasDisponibles.Remove(factura[k].Empresa.text);
                }
            }
        }
        foreach (var button in valueButtons)
        {
            valuesLabel.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }

        int x=0;
        foreach (var monto  in pagos[0].montos )
        {
            valuesLabel[x].text = monto.ToString();
            x++;
        }
        foreach (var button in facturaButtons)
        {
            facturaLabel.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }
        for (int i = 0; i < pagos.Count; i++)
        {
            facturaLabel[i].text = pagos[i].empresa;
        }

        currentFacturaText.text = pagos[0].empresa;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
            timer -= Time.deltaTime;
        TimerText.text = timer.ToString("00");
        if (timer <= 0||TaskCompleted==3)
        {
            timer = 0;
            ExitTimer -= Time.deltaTime;
            Debug.Log(PagosCorrectos);
            if (ExitTimer <= 0)
            {
                if(TaskCompleted<Results.Count)
                    PlayerManager.Instance.ApplyActivity(Results[TaskCompleted]);
                else
                    PlayerManager.Instance.ApplyActivity(Results[Results.Count-1]);
                SceneManager.LoadScene(0);
            }
        }
    }

    public void ChangeFactura(int i)
    {
        int j = 0;
        foreach (var monto  in pagos[i].montos )
        {
            valuesLabel[j].text = monto.ToString();
            j++;
        }
        CurrentFactura = i;
        currentFacturaText.text = pagos[i].empresa;
        foreach (var button in valueButtons)
        {
            button.interactable = true;
        }
    }
    public void Pagar(int i)
    {
        TaskCompleted++;
        if (pagos[CurrentFactura].montoCorrecto == i)
        {
            PagosCorrectos++;
            facturaButtons[CurrentFactura].interactable = false;
            facturaButtons[CurrentFactura].GetComponent<Image>().color = Color.forestGreen;
        }
        else
        {
            PagosCorrectos--;
            
            facturaButtons[CurrentFactura].interactable = false;
            facturaButtons[CurrentFactura].GetComponent<Image>().color = Color.darkRed;
        }
        foreach (var button in valueButtons)
        {
            button.interactable = false;
        }
    }
}

public class Pago
{
    public string empresa;
    public List<int> montos=new List<int>();
    public int montoCorrecto;
}
public class Factura
{
    public string empresa;
    int monto;
}