import React, { Component } from "react";

//import UserService from "../services/user.service";

export default class Home extends Component {
  constructor(props) {
    super(props);

  }

  render() {
    return (
      <div className="container">
        <header className="jumbotron">
          <h3>File sharing Web App</h3>
          <p>Upload and share files with other users. Built using React, ASP.NET core and MongoDB with GridFS. <a href="https://github.com/enriquebarba97/FilesApi">Github Repo</a></p>
        </header>
      </div>
    );
  }
}
