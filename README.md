# Chrome Cookies Exporter (cookies.txt)
:star: Star us on GitHub - it motivates us a lot!

This program runs on Windows and it directly read your Chrome's cookies database and generates a `cookies.txt` in Netscape cookie jar syntax. The generated `cookies.txt` can be used in `youtube-dl`, `cURL`, and many other utilities. It looks like this:

![](https://i.imgur.com/19aex58.png)


## Installation

* Clone this repo and compile. 
* Or download directly from the release page.


## Usage

1. Start the program. 
2. It will detect the default Chrome installation and find the cookie database. 
3. If it failed to detect the installation, use the `Choose...` button to select the cookies database manually.
4. Type something in the host filtering text box. Use either regex or literal to match the filter.
5. Click `Save To...` save the result to a cookies.txt file.


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.


## License
[MIT](https://choosealicense.com/licenses/mit/)