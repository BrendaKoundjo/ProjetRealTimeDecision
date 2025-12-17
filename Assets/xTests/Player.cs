using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float m_TranslationSpeed;
    [SerializeField] float m_RotationSpeed;

    [SerializeField] GameObject m_BallPrefab;
    [SerializeField] Transform m_BallSpawnPos;
    [SerializeField] float m_BallInitTranslationSpeed;

    Rigidbody m_Rb;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
    }

    public void ShootBall()
    {
        GameObject newBallGO = Instantiate(m_BallPrefab);
        newBallGO.transform.position = m_BallSpawnPos.position;
        newBallGO.GetComponent<Rigidbody>().linearVelocity = m_BallSpawnPos.forward * m_BallInitTranslationSpeed;
    }
    // Update is called once per frame
    /*
     * void Update()   
    {

        // kinematic motion
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        //transform.position += transform.forward * m_TranslationSpeed * Time.deltaTime;
        transform.Translate(new Vector3(0,0, vInput*m_TranslationSpeed * Time.deltaTime), Space.Self);

        //transform.rotation = Quaternion.AngleAxis(hInput*m_RotationSpeed * Time.deltaTime,transform.up) * transform.rotation;
        transform.Rotate(new Vector3(0, 1, 0), hInput * m_RotationSpeed * Time.deltaTime, Space.Self);

    }*/

    /* void FixedUpdate()
     {
         //dynamic motion
         float vInput = Input.GetAxis("Vertical");
         float hInput = Input.GetAxis("Horizontal");

         //mode positionnel
         m_Rb.MovePosition(m_Rb.position + vInput*transform.forward * m_TranslationSpeed * Time.fixedDeltaTime);
         m_Rb.MoveRotation(Quaternion.AngleAxis(hInput * m_RotationSpeed * Time.fixedDeltaTime,
             transform.up) * m_Rb.rotation);

     }*/
}
