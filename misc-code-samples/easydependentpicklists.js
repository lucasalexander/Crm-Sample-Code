/**
 * @summary	Easy dependent picklists for Dynamics CRM.
 * @author	Lucas Alexander
 * @link	http://www.alexanderdevelopment.net
 */


/** global array to hold the picklist hierarchies and data  */
var _filteredPicklists = [];

/**
 * Intializes a dependent picklist hierarchy on a CRM form.
 *
 * @param picklists - pipe-delimited string that contains the picklists in the hierarchy in order of first parent to last child 
 * example:"new_picklistlevel1|new_picklistlevel2|new_picklistlevel3" .
 * 
 * @param separator - string used to separate levels in the parent-child option tree
 * example: " > "
 * 
 * @param optionTree - array containing all possible parent-child option paths you want to enable on the form
 * example: ["1 -> 1a -> 1a1","1 -> 1a -> 1a2","1 -> 1b -> 1b1","1 -> 1c","2 -> 2a -> 2a1"]
 *  
 */
function initializePicklistFilter(picklists, separator, optionTree){
	var hierarchy = picklists.split("|");
	hierarchy.forEach(function(picklistName){
		//add the filterOptions to onchange event for every picklist in the hierarchy
		Xrm.Page.getAttribute(picklistName).addOnChange(filterOptions);
	});

	//instantiate new filteredpicklist object and add it to the global array
	var filteredPicklist = {
		"separator":separator,
		"optionTree":optionTree,
		"hierarchy":hierarchy
	};
	_filteredPicklists.push(filteredPicklist);

	//fire the onchange event for the top level optionset in the hierarchy to apply the initial filter
	Xrm.Page.getAttribute(hierarchy[0]).fireOnChange();
}

/** Applies the filtering to a dependent picklist hierarchy onchange of one of the hierarchy members. */
function filterOptions(executionContext){
	//get the name of the picklist that just changed
	var callingControlName = executionContext.getEventSource().getName();

	//get the filteredpicklist object that contains the calling picklist
	var currentFilteredPicklist = getFilteredPicklist(callingControlName, _filteredPicklists);
	if(!currentFilteredPicklist){
		console.log("Error");
		return;
	}
	
	//get the filteredpicklist values
	var hierarchy = currentFilteredPicklist.hierarchy;
	var optionTree = currentFilteredPicklist.optionTree;
	var separator = currentFilteredPicklist.separator;

	//create an array to hold the selected options for each picklist in the hierarchy
	var selectedOptions = [];
	
	//loop through the hierarchy to get the selected options, clear child options and temporarily disable the onchange event for each picklist
	for(var i=0;i<hierarchy.length;i++){
		//get the picklist name
		var picklistName = hierarchy[i];

		//temporarily remove onchange event
		Xrm.Page.getAttribute(picklistName).removeOnChange(filterOptions);

		//get the currently selected option and store it for use later (will be null if nothing is selected)
		var selectedOption = Xrm.Page.getAttribute(picklistName).getSelectedOption();
		selectedOptions.push(selectedOption);

		//clear the picklist options for every child picklist
		if(i>0){
			Xrm.Page.getControl(picklistName).clearOptions();
		}
	}
	
	//represents the full path from first parent to last child - start with empty value and will build out by looping
	var workingPath = "";

	//loop through all parent picklists to set the corresponding child options
	for(var i=0;i<hierarchy.length-1;i++){
		//get the picklist name
		var picklistName = hierarchy[i];
		
		//get the childpicklist name
		var childPicklistName = hierarchy[i+1];

		//get the currently selected option
		var selectedOption = selectedOptions[i];

		//get the currently selected child option
		var selectedChildOption = selectedOptions[i+1];

		//if the currently selected option is null, we can stop
		if(!selectedOption){
			break;
		}
		else{
			//get the text for the selected option
			var selectedText = selectedOption.text;

			//build up the working path with this level's selected option text
			workingPath = workingPath + selectedText + separator;

			//instantiate an array to keep track of options already added so we don't add duplicates
			var alreadyAdded = [];
					
			//find options in the optiontree array that start with the workingpath value
			optionTree.forEach(function(optionPath){
				if(optionPath.indexOf(workingPath)==0) {
					//figure out which level we are in and find the corresponding options to add to the appropriate picklist
					var pathComponents = optionPath.split(separator);
					var optionTextToAdd = pathComponents[i+1];

					//assume the previously selected child option is not present in our "new" picklist options
					var selectedOptionPresent = false;

					//see if there is valid corresponding option in the child picklist to add
					var optionToAdd = getOptionByText(Xrm.Page.getAttribute(childPicklistName).getOptions(),optionTextToAdd);
					if(optionToAdd){
						if(alreadyAdded.indexOf(optionToAdd.text)<0){
							Xrm.Page.getControl(childPicklistName).addOption(optionToAdd);
							alreadyAdded.push(optionToAdd.text);

							//if previously selected child option is present in our "new" picklist options, set flag
							if(selectedChildOption){
								if(optionToAdd.text==selectedChildOption.text){
									selectedOptionPresent = true;
								}
							}
						}
					}

					//if we had a selected option in the picklist before we started, set it back
					if(selectedOptionPresent){
						Xrm.Page.getAttribute(childPicklistName).setValue(parseInt(selectedChildOption.value));
					}
				}
			});
		}
	}

	//add onchange eventhandler back to picklists in this hierarchy
	for(var i=0;i<hierarchy.length;i++){
		Xrm.Page.getAttribute(hierarchy[i]).addOnChange(filterOptions);
	}
}

/** Finds the filteredpicklist object that contains a picklist attribute and returns it (or null). */
function getFilteredPicklist(picklistName, filteredPicklists){
	var returnvalue = null;
	filteredPicklists.forEach(function(filteredPicklist){
		if(filteredPicklist.hierarchy.indexOf(picklistName)>-1)
		{
			returnvalue = filteredPicklist;
		}
	});
	return returnvalue;
}

/** Checks if a set of picklist options contains a text value and returns the found option (or null). */
function getOptionByText(options, optiontext){
	var returnoption = null;
	options.forEach(function(option){
		if(option.text==optiontext){
			returnoption = option;
		}
	});
	return returnoption;
}