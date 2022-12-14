using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player1Blocks : MonoBehaviour
{
    public GameObject tetromino; //parent tetromino game object
    bool active, droppable, movable, rotatable, cleared; //stores tetromino states
    int count = 0, columnsCleared;
    Gameplay gameControl;
    public string Loss;

    // Start is called before the first frame update
    void Start()
    {
        active = true; //tetromino initially active
        gameControl = FindObjectOfType<Gameplay>(); //instantiates gameplay class to access members
        Loss = "Player2Win";
    }

    // Update is called once per frame
    void Update()
    {
        if(active && !Pause.is_paused) //only allows movement for active tetromino while not paused
        {
            //movement
            if(Input.GetKeyDown(KeyCode.W)) //move tetromino up
            {   
                movable = MovementCheckUp();

                if(movable)
                {
                    tetromino.transform.position += new Vector3(0, 1, 0); //move by one space in +y direction
                }

            }
            
            if(Input.GetKeyDown(KeyCode.S)) //move tetromino down
            {
                movable = MovementCheckDown();

                if(movable)
                {
                    tetromino.transform.position += new Vector3(0, -1, 0); //move by 1 space in -y direction
                }
            }

            //rotation
            if(Input.GetKeyDown(KeyCode.A)) //rotate tetromino 90 degrees
            {
                tetromino.transform.eulerAngles -= new Vector3(0, 0, 90); //rotate tetromino

                rotatable = RotationCheck(); //check if that rotation was valid
                if(!rotatable)
                {
                    tetromino.transform.eulerAngles += new Vector3(0, 0, 90); //revert rotation if that rotation was invalid;
                }
            }

            //dropping
            if(Input.GetKey(KeyCode.D) || count >= Gameplay.timer) //drop tetromino if D key is pressed or auto drop count reached
            {
                columnsCleared = 0; //reset variable

                if(count >= Gameplay.timer) //reset auto drop counter
                {
                    count = 0;
                }

                droppable = DropCheck(); //check if tetromino can drop
                
                if(droppable)
                {
                    tetromino.transform.position += new Vector3(1, 0, 0); //move in +x direction if droppable
                }
                else
                {
                    active = false; //if not droppable it sets

                    SetBlocks(); //register location of inactive blocks

                    foreach(Transform block in tetromino.transform) //loops through blocks
                    {
                        cleared = gameControl.ClearColumn((int)Mathf.Round(block.transform.position.x)); //checks if any of the columns are complete

                        if(cleared)
                        {
                            ShiftBlocks((int)Mathf.Round(block.transform.position.x)); //if a column was cleared shift blocks
                            columnsCleared ++; //keep count of columns cleared with this tetromino
                        }
                    }

                    if(columnsCleared > 2) // if 3 or 4 columns cleared at once call attack function
                    {
                        Attack(columnsCleared);
                    }

                    gameControl.P1SpawnTetromino(); //spawn new tetromino
                }
            }
            count++; //increment auto drop counter
        }
    }

    bool DropCheck() //checks if it possible for the tetromino to continue dropping
    {
        foreach(Transform block in tetromino.transform)
        {
            if(Mathf.Round(block.transform.position.x) == 19) //can't drop if block is at the divider
            {
                return false;
            }
            else if(Gameplay.blocks[(int)Mathf.Round(block.transform.position.y), (int)Mathf.Round(block.transform.position.x + 1)] != null) //can't drop if there is a set block in the way
            {
                return false;
            }
        }

        return true;
    }

    bool MovementCheckUp() //checks if tetromino can move up
    {
        foreach(Transform block in tetromino.transform) //loops through blocks
        {
            if(block.transform.position.x >= 0) //only check blocks that are in bounds to avoid errors
            {
                if(Mathf.Round(block.transform.position.y) == 9) //cant move it at upper edge
                {
                    return false;
                }
                else if(Gameplay.blocks[(int)Mathf.Round(block.transform.position.y + 1), (int)Mathf.Round(block.transform.position.x)] != null) //cant move if there is a set block in the way
                {
                    return false;
                }
            }
        }
        return true; //if none of the blocks cant move return true
    }

    bool MovementCheckDown()
    {
        foreach(Transform block in tetromino.transform) //loops through blocks
        {
            if(block.transform.position.x >= 0) //only check blocks that are in bounds to avoid errors
            {
                if(Mathf.Round(block.transform.position.y) == 0) //cant move if at lower edge
                {
                    return false;
                }
                else if(Gameplay.blocks[(int)Mathf.Round(block.transform.position.y - 1), (int)Mathf.Round(block.transform.position.x)] != null) //cant move if there is a set block in the way
                {
                    return false;
                }
            }
        }
        return true; //if none of the blocks cant move return true
    }

    bool RotationCheck() //check if rotations are valid
    {
        foreach(Transform block in tetromino.transform)
        {
            if(Mathf.Round(block.transform.position.y) < 0) //rotation invalid if block is below playfield
            {
                return false;
            }
            else if(Mathf.Round(block.transform.position.y) > 9) //rotation invalid if block is above playfield
            {
                return false;
            }
            else if(Gameplay.blocks[(int)Mathf.Round(block.transform.position.y), (int)Mathf.Round(block.transform.position.x)] != null) //rotation invalid if block intersects set block
            {
                return false;
            }
        }

        return true; //if none of above are true the rotation is valid
    }

    void SetBlocks()
    {
        foreach(Transform block in tetromino.transform)
        {
            if((int)Mathf.Round(block.transform.position.x) >= 0) //checks if the blocks have reached the end of the screen
            {
                Gameplay.blocks[(int)Mathf.Round(block.transform.position.y), (int)Mathf.Round(block.transform.position.x)] = block; //store each block in blocks array at their set location
            }
            else
            {
                SceneManager.LoadScene(Loss); //if end of screen reached show victory screen for opponent
            }
        }

        if(Gameplay.timer > 1)
        {
            Gameplay.timer -= 0.25; //speed up blocks slightly whenever a tetromino is set
        }
    }

    void ShiftBlocks(int column)
    {
        for(int i = 0; i < 10; i++)
        {
            for(int j = (column - 1); j >= 0; j--) // loops through blocks in columns to left of cleared column
            {
                if(Gameplay.blocks[i, j] != null) //if there is a set block in that space move it to the right
                {
                    Gameplay.blocks[i, j + 1] = Gameplay.blocks[i, j]; //copies block info
                    Gameplay.blocks[i, j + 1].gameObject.transform.position += new Vector3(1, 0, 0); //moves block object
                    Gameplay.blocks[i, j] = null; //sets blocks previous location to empty
                }
            }
        }
    }

    void Attack(int columns) //shifts blocks towards opponent if 3 or 4 columns are cleared at a time
    {
        for(int i = 0; i < 10; i++)
        {
            for(int j = 39; j >= 0; j--) // loops through blocks in columns to right of cleared column
            {
                if(Gameplay.blocks[i, j] != null) //if there is a set block in that space move it to the left
                {
                    Gameplay.blocks[i, j + (columns - 2)] = Gameplay.blocks[i, j]; //copies block info
                    Gameplay.blocks[i, j + (columns - 2)].gameObject.transform.position += new Vector3((columns - 2), 0, 0); //moves block object
                    Gameplay.blocks[i, j] = null; //sets blocks previous location to empty
                }
            }
        }
    }
}