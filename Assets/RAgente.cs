using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RAgente : Agent
{

    public bool groundCheck = false;
    public Transform positionCheck;
    public LayerMask layerMask;


    Rigidbody rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Objetivo;
    public override void OnEpisodeBegin()
    {
        //si te caes este va a ser tu punto de inicio
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        //mover el objetivo dentro del plano de manera aleatoria
        Objetivo.localPosition = new Vector3(Random.value * 20 - 4, 0.5f, Random.value * 8 - 4);
    }

    //funcion para programar los sensores
    public override void CollectObservations(VectorSensor sensor)
    {
        //el agente sepa la posicion del objetivo
        sensor.AddObservation(Objetivo.localPosition); //3 observaciones
        sensor.AddObservation(this.transform.localPosition); //3 observaciones

        //la velocidad del agente
        sensor.AddObservation(rBody.velocity.x);//1 observacion
        sensor.AddObservation(rBody.velocity.y);//1 Observacion Adicional
        sensor.AddObservation(rBody.velocity.z);//1 observacion
    }
    //funcion de acciones y politicas
    public float multiplicador = 10;
    
    public float jumpForce = 12.0f;
    public float maxJumpHeight = 10.0f; // Altura máxima de salto permitida
    public int maxAirFrames = 60; // Número máximo de fotogramas en el aire (a 60 FPS, esto sería equivalente a 2 segundos)
    private int currentAirFrames = 0; // Fotogramas actuales en el aire


    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        //programar 2 actuadores
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        controlSignal.y = actions.ContinuousActions[2];
        rBody.AddForce(controlSignal * multiplicador);

        //groundCheck = Physics.CheckSphere(positionCheck.position, 0.3f, layerMask);

        // Detectar si el agente debe saltar
        if (groundCheck && ShouldJump(actions))
        {
            groundCheck = false;
            rBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // currentAirFrames++;
            if (currentAirFrames > maxAirFrames)
            {
                SetReward(-0.1f); // Penalización por estar en el aire demasiado tiempo
            }else{
                currentAirFrames = 0;
            }
        }

        //programar las politicas
        float distanciaobjetivo = Vector3.Distance(this.transform.localPosition, Objetivo.localPosition);

        //politica para cuando el agente agarre al objetivo
        if (distanciaobjetivo < 1.42f)
        {
            SetReward(1.2f);
            EndEpisode();
        }

        //politica en caso de que el agente sea tan pendejo y se caiga
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-2.0f);
            EndEpisode();
        }
        /*else if (this.transform.localPosition.y > 5)
        {
            SetReward(-2.0f);
            EndEpisode();
        }*/

        SetReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var conti = actionsOut.ContinuousActions;
        conti[0] = Input.GetAxis("Horizontal");
        conti[1] = Input.GetAxis("Vertical");
    }

    private bool ShouldJump(ActionBuffers actions)
    {
        // Aquí decides si el agente debe saltar basándote en las acciones recibidas.
        // Por ejemplo, puedes verificar si el valor de actions.ContinuousActions[3]
        // (suponiendo que es tu acción de salto) supera un cierto umbral.
        return actions.ContinuousActions[2] > 0.5f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        groundCheck = true;
    }
}