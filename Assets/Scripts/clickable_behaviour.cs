using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class clickable_behaviour : MonoBehaviour
{
    [SerializeField]
    private Boolean isTiming;
    private int timer;
    private SoundEffect _clickSound;
    private SpriteRenderer spriteRenderer; 

    public Boolean IsClicking = false;
    public Boolean isclicked;
    public ParticleController particleSpawner;
    

    string[] skins = new string[] { "Skin1", "Skin2", "Skin3", "Skin4" }; 

    private Camera _camera;
    private Collider2D _selfCollider;

    void Start()
    {
        _clickSound = GetComponent<SoundEffect>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _camera = Camera.main;
        _selfCollider = GetComponent<Collider2D>();
    }

    void Update()
    {

        spriteRenderer.sprite = Resources.Load<Sprite>(skins[GameData.GetSkin()]);

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
        // Any key counts as a click too (see the keyboard hook), so both inputs are
        // treated the same way here. Neither may earn score while a ransom is running.
        bool wantsToClick = false;
        if (!GameData.ScoreBlocked)
        {
            bool mouseClick = Input.GetMouseButtonDown(0) && PointerIsOverThisObject();
            bool keyPress = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            wantsToClick = mouseClick || keyPress;
        }

        if (wantsToClick)
        {
            isclicked = true;

            // Bank the click straight away. The score used to be counted by
            // Score_Noting polling the isclicked flag on the same frame, and
            // if that script's Update ran first the click was lost entirely.
            GameData.AddClick();

            if (particleSpawner != null)
            {
                particleSpawner.Trigger();
            }

            isTiming = true;
            timer = 0;
            IsClicking = true;
            transform.localScale = new Vector2(0.8f, 0.8f);

            if (_clickSound != null)
            {
                _clickSound.Play();
            }
        }
        else
        {
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

    /// <summary>
    /// True only when the pointer sits on this object's own collider.
    ///
    /// This used to accept any collider under the cursor, which was fine while the
    /// Galo was the only collider in the scene. Now the glitched cloud and the
    /// ransom coins exist too, so an unmatched hit would quietly score for them.
    /// </summary>
    private bool PointerIsOverThisObject()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera == null || _selfCollider == null)
        {
            return false;
        }

        Vector2 pointer = _camera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(pointer);
        return hit != null && hit == _selfCollider;
    }
}
