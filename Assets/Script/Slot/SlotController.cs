using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlotController : MonoBehaviour
{
	// const
	const int SLOT_STATE_IDLE = 0;
	const int SLOT_STATE_SPIN = 1;
	const int SLOT_STATE_STOP = 2;
	public const float LINE_STOP_INTERVAL = 0.1f;

	// ui
	public Button spinBtn;
	public Text winText;
	public Text totalValText;
	public Text betTimeText;
	public GameObject losUi;
	public GameObject winUi;
	public GameObject helpUi;
	public AudioSource spinWheel;
	public AudioSource matchSound;
	// property
		public GameObject cellPrefab;
	public int cellSpinState = 0;
	public Vector3 firstCellPos = new Vector3 (-2f, -2.5f, 0f);
	public Vector3 secondCellPos = new Vector3 (-0.6f, -2.5f, 0f);
	public Vector3 thirdCellPos = new Vector3 (0.8f, -2.5f, 0f);
	Vector3[] cellPos;
	SlotCell[] cells;

	float spinTime;

	int betTimes;
	int totalPrice;
	int targetPrice;
	// other
	Hero hero;

	void Start ()
	{		
		cellPos = new Vector3[] { firstCellPos, secondCellPos, thirdCellPos };
		cells = new SlotCell[cellPos.Length];
		for (int i = 0; i < cellPos.Length; i++) {
			GameObject cell = (GameObject)Instantiate (cellPrefab, cellPos [i], Quaternion.identity);
			cells [i] = cell.GetComponent<SlotCell> ();
			cells [i].SetSymbol (Random.Range (0, 9));
		}
		betTimes = 10;
		totalPrice = 0;
		targetPrice = 40;
		
		// find hero
		// hero = GameObject.FindGameObjectWithTag("Hero").GetComponent<Hero> ();
	}

	void Update ()
	{
		winText.text = targetPrice.ToString();
		totalValText.text = totalPrice.ToString();
		betTimeText.text = betTimes.ToString() + " Bet Times";
		if (cellSpinState == SLOT_STATE_SPIN) {
			for (int i = 0; i < cells.Length; i++) {
				SlotCell cell = cells [i];
				if (cell.isSpin == false) {
					cells [i].StartSpin ();
				} 
			}
		} else if (cellSpinState == SLOT_STATE_STOP) {
			cellSpinState = SLOT_STATE_IDLE;
			if (cells [0].isSpin == true) {
				StartCoroutine (
					StopLines (() => {
						HighlightSlotResults (GetSlotResults ());
						if(betTimes == 0 ) {
							if(targetPrice > totalPrice){
								losUi.SetActive(true);
							}else{
								winUi.SetActive(true);
							}
							
						}
					}));
			}
		}
		Debug.Log(betTimes);
		Debug.Log(cellSpinState);
		
		// date.setTime(date.getTime());
		// Debug.Log(spinTime);
		// Debug.Log(Time.time);
		if((Time.time - spinTime) > 1 && spinTime != 0.0f) {
			cellSpinState = SLOT_STATE_STOP;
			spinTime = 0.0f;
		}
	}

	public void reload(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void onHelp(){
		helpUi.SetActive(true);
	}
	public void closeHelp(){
		helpUi.SetActive(false);
	}

	public void SwitchSpin ()
	{
		if(betTimes > 0){
			if (cellSpinState == SLOT_STATE_IDLE) {
				cellSpinState = SLOT_STATE_SPIN;
				spinWheel.Play(0);
				betTimes -= 1;
				spinTime = Time.time;
			} else if (cellSpinState == SLOT_STATE_SPIN) {
				cellSpinState = SLOT_STATE_STOP;
				spinTime = 0.0f;
			}
		}
	}

	public void StopSpin ()
	{
		cellSpinState = SLOT_STATE_STOP;
	}

	IEnumerator StopLines (System.Action callback)
	{
		for (int i = 0; i < cells.Length; i++) {
			SlotCell cell = cells [i];
			if (cell.isSpin == true) {
				cell.StopSpin ();
				yield return new WaitForSeconds (LINE_STOP_INTERVAL);
			}
		}
		yield return new WaitForSeconds (LINE_STOP_INTERVAL + SlotCell.STOP_IMPEDENCE_DELAY_TIME);
		callback ();
	}

	Hashtable GetSlotResults ()
	{
		Hashtable results = new Hashtable ();
		for (int i = 0; i < cells.Length; i++) {
			int symbol = cells [i].symbol;
			if (results.ContainsKey (symbol)) {
				results [symbol] = (int)results [symbol] + 1;
			} else {
				results [symbol] = 1;
			}
		}
		return results;
	}

	void HighlightSlotResults (Hashtable slotResults)
	{
		foreach (DictionaryEntry result in slotResults) {
			if (result.Value.Equals (2)) {
				for (int i = 0; i < cells.Length; i++) {
					if (cells [i].symbol == (int)result.Key) {
						cells [i].HighLight ();
					}
				}
				totalPrice += 10;
				matchSound.Play();
				// trigger hero spell
				// hero.DoSpell ();
			} else if (result.Value.Equals (3)) {
				for (int i = 0; i < cells.Length; i++) {
					if (cells [i].symbol == (int)result.Key) {
						cells [i].SuperHighLight ();
					}
				}
				totalPrice += 20;
				matchSound.Play();
				// trigger super spell
				// hero.DoSpell ();
			} 
		}
	}
}