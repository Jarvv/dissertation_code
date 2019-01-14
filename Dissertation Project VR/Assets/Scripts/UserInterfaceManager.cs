using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceManager : MonoBehaviour {

	private Text header, rmsText, transformationText;
	private GameObject acceptButton, rejectButton, resultsUi;

	private void Start(){
		resultsUi = this.transform.Find ("ResultsUI").gameObject;
		header = resultsUi.transform.Find("Header").GetComponent<Text>();
		rmsText = resultsUi.transform.Find("RMS").GetComponent<Text>();
		transformationText = resultsUi.transform.Find("Transformation").GetComponent<Text>();

		acceptButton = this.transform.Find ("Accept Result Button").gameObject;
		rejectButton = this.transform.Find ("Reject Result Button").gameObject;
	}
		
	// Loads the GUI to appear when the matching proccess has finished.
	public void LoadUI(string rmse, Matrix4x4 trs, string noiseRmse){
		this.gameObject.SetActive (true);

		Start ();

		// Accept if the rmse value is below 1, if it's higher then usually something has gone
		// wrong.
		if (float.Parse (rmse) > 1.0f) {
			header.text = "A Transformation was not found";
			rmsText.text = "";
			transformationText.text = "";
			rejectButton.gameObject.SetActive(false);
			acceptButton.gameObject.SetActive(true);

		} else {
			header.text = "A transformation was found:";
			rmsText.text = "The RMSE value is: " + rmse;
			transformationText.text = "The actual RMSE value was: " + noiseRmse.ToString();
			acceptButton.gameObject.SetActive(true);
			rejectButton.gameObject.SetActive(true);

		}

	}


}
