Here is the flow

	1. Main Window is bootstrapped by app
		Main Window contains logic for login/logout/grid to display assigned project

	2. Main Window > Managetimeline
		It is basically the heart of whole application. It contains all the UC developed by all devs

	3. designer (managed by KG) - used for Form event design
	4. DesignImager (managed by KG) - used t convert XML to image so that can be saved as is in DB/Server
	5. DesignViewer (managed by KG) - used in edit mode form events
	6. SQLite_library (managed by KG) - contains all sqlite logic
	7. VideoCreator (managed by KG) - bootstrap project
	8. VideoCreatorXAMLLibrary (managed by KG) 
		DLL created to reduce burden on VideoCreator project
		a. AuthAPIViewModel - VC side code to call the server APIs
		b. VideoCreatorAuthHelper - Contains common and basic features to called server API
		c. Designer_UserControl.xaml - XAML Wrapper to call designerd UC
		d. DesignImager_UserControl - XAML Wrapper to call DesignImager UC
		e. Helpers - contains logic to create heler functions
		f. Icons Folder contain all image
		g. Images - Contains some images (may or may not be in use)
		h. Media - contains some medial mp3/jpeg etc (may or may not be in use)
		i. Loader Folder contains XAML for showing loaders
		j. MediaLibraryData - contains XAML for downloading images from CB libs
		k. Models - contains few models
		l. Preview - contains preview UC

	9. ServerApiCall_UserControl (managed by KG) - contains logic to call server API
