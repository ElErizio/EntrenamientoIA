using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RAgent : Agent
{
    Rigidbody rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Objetivo;
    public Transform paloCiego;

    public LayerMask mask;
    public LayerMask mask2;
    public bool isGrounding = false;
    //bool salto = false;
    int countGoals;
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
        if ((countGoals < 3) || (countGoals % 2 == 0))
        {
            Objetivo.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        }
        else
        {
            //Objetivo.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
            Objetivo.localPosition = new Vector3(0.13f, .5f, 12);
        }
    }

    //funcion para programar los sensores
    public override void CollectObservations(VectorSensor sensor)
    {
        //el agente sepa la posicion del objetivo
        sensor.AddObservation(Objetivo.localPosition); //3 observaciones
        sensor.AddObservation(this.transform.localPosition); //3 observaciones

        //la velocidad del agente
        sensor.AddObservation(rBody.velocity.x);//1 observacion
        sensor.AddObservation(rBody.velocity.z);//1 observacion

        sensor.AddObservation(isGrounding);
    }
    //funcion de acciones y politicas
    public float multiplicador = 10;
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);



        isGrounding = Physics.CheckSphere(paloCiego.position, 0.1f, mask);

        /*if (Physics.CheckSphere(paloCiego.position, 0.1f, mask2))
            {
                salto = false;
                SetReward(0.5f);
                print("Yei");
            }
        */

        //programar 2 actuadores
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rBody.AddForce(controlSignal * multiplicador);


        if (isGrounding)
        {
            Vector3 jumpVector = Vector3.zero;
            jumpVector.y = actions.ContinuousActions[2];
            rBody.AddForce(jumpVector * 500);
        }

        Quaternion targetRotation = Quaternion.LookRotation(rBody.velocity.normalized, Vector3.up);
        Quaternion yOnlyRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        transform.rotation = yOnlyRotation;

        //programar las politicas
        float distanciaobjetivo = Vector3.Distance(this.transform.localPosition, Objetivo.localPosition);


        //politica para cuando el agente agarre al objetivo
        if (distanciaobjetivo < 1.42f)
        {
            SetReward(1.0f);
            countGoals++;
            EndEpisode();
        }
        //politica en caso de que el agente sea tan pendejo y se caiga

        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-2.0f);
            EndEpisode();
        }
        SetReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var conti = actionsOut.ContinuousActions;
        conti[0] = Input.GetAxis("Horizontal");
        conti[1] = Input.GetAxis("Vertical");
        conti[2] = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
    }
}