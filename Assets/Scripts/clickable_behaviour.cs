using System;
using UnityEngine;

public class clickable_behaviour : MonoBehaviour
{
    [SerializeField]
    private Boolean isTiming;
    private int timer;
    private SoundEffect _clickSound;

    public Boolean IsClicking = false;
    public Boolean isclicked;
    public ParticleController particleSpawner;


    void Start()
    {
        _clickSound = GetComponent<SoundEffect>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D clicked = Physics2D.Raycast(mousePos, Vector2.zero);
            if (clicked.collider != null)
            {
                isclicked = true;

                Debug.Log("clicked");

                // Bank the click straight away. The score used to be counted by
                // Score_Noting polling the isclicked flag on the same frame, and
                // if that script's Update ran first the click was lost entirely.
                GameData.AddClick();
                particleSpawner.Trigger();
                isTiming = true;
                timer = 0;
                IsClicking = true;
                transform.localScale = new Vector2(0.8f, 0.8f);

                if (_clickSound != null)
                {
                    _clickSound.Play();
                }
            }
        }
        else{

            isclicked = false;

        }

        if (isTiming)
        {
            timer += 1;
        }
        if (timer >= 15)
        {
            timer = 0;
            isTiming = false;

            IsClicking = false;
            transform.localScale = new Vector2(0.9f, 0.9f);
        }
    }
}
