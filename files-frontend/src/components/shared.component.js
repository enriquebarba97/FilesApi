import React, { Component } from "react";

import AuthService from "../services/auth.service";
import FileService from "../services/files.service";
export default class Shared extends Component {

    constructor(props) {
        super(props);

        this.state = {
            currentUser: AuthService.getCurrentUser(),
            files: null,
            message: "",
            error: ""
        };
    }

    
    componentDidMount() {
        FileService.getSharedFiles(this.state.currentUser.username).then(
          response => {
            this.setState({
              files: response.data
            });
          },
          error => {
            this.setState({
              error:
                (error.response && error.response.data) ||
                error.message ||
                error.toString()
            });
          }
        );
    }

    downloadFile(filename, owner){
        FileService.getFile(filename,owner).then(
            response => {
                const blob = new Blob([response.data]);
                console.log(blob.size);
                const url = window.URL.createObjectURL(new Blob([response.data]));
                const link = document.createElement('a');
                link.href = url;
                link.setAttribute('download', filename);

                document.body.appendChild(link);
                link.click();

                link.parentNode.removeChild(link);
                
            },
            error => {
              this.setState({
                message: "Something went wrong"
              });
            }
          );
    }

    render() {
        const {files,currentUser,message,error} = this.state;
      return (
        <div className="container">
            {currentUser && 
            <h3>Shared with {currentUser.username}</h3>}

            {message && <div className="alert alert-success" role="alert">{message}</div>}
            {error && <div className="alert alert-danger" role="alert">{error}</div>}
            

          <div className="list-group">
              {files && files.map((file,index) =>
              <li key={file.filename} className="list-group-item justify-content-between align-items-center">
                <div className="container">
                <div className="row">
                <div className="col-10">{file.filename} - {file.owner}</div>
                <div className="col-2 btn-group">
                <button onClick={() => this.downloadFile(file.filename,file.owner)} className="btn btn-primary">Download</button>
                </div>
                </div>
                </div>
              </li>)}
            </div>
        </div>
      );
    }



}