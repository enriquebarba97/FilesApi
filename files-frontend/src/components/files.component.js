import React, { Component } from "react";
import Form from "react-validation/build/form";
import Input from "react-validation/build/input";
import CheckButton from "react-validation/build/button";

import AuthService from "../services/auth.service";
import FileService from "../services/files.service";

const required = value => {
    if (!value) {
        return (
            <div className="alert alert-danger" role="alert">
                This field is required!
            </div>
        );
    }
};

export default class Files extends Component {
    constructor(props) {
      super(props);
  
      this.state = {
        fileUp: null,
        files: null,
        currentUser: AuthService.getCurrentUser(),
        message: "",
        error: "",
        shareUser: "",
        shareForm: null
      };
    this.onFileUpload = this.onFileUpload.bind(this);
    this.onChangeFile = this.onChangeFile.bind(this);
    this.onChangeShare = this.onChangeShare.bind(this);
    this.onShareSubmit = this.onShareSubmit.bind(this);
    }
  
    componentDidMount() {
      FileService.getOwnFiles(this.state.currentUser.username).then(
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

    refreshFiles() {
        FileService.getOwnFiles(this.state.currentUser.username).then(
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

    onChangeFile(e) {
        this.setState({fileUp:e.target.files[0]})
    }

    onFileUpload(e){
        e.preventDefault() // Stop form submit
        this.setState({
            message: "",
            loading: true
        });

        this.form.validateAll();

        const formData = new FormData();
        formData.append('file',this.state.fileUp);
        FileService.uploadFile(formData, this.state.currentUser.username).then((response)=>{
          this.setState({
              message: "File uploaded successfully",
              loading: false
          })
          this.refreshFiles();
        });
      }

    downloadFile(filename){
        FileService.getFile(filename,this.state.currentUser.username).then(
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

    deleteFile(filename){
        FileService.deleteFile(filename,this.state.currentUser.username).then(
            response => {
                this.setState({
                    error: "",
                    message: "File deleted"
                });
                this.refreshFiles();
            },
            error => {
                this.setState({
                    error: "Someting went wrong",
                    message: ""
                });
            }
        )
    }

    // File sharing functions

    setShareForm(index) {
        this.setState({shareForm:index});
    }

    onChangeShare(e) {
        this.setState({shareUser: e.target.value})
    }

    onShareSubmit(e) {
        e.preventDefault();
        console.log(this.fileShare.value)
        FileService.addShare(this.fileShare.value,this.state.currentUser.username,this.state.shareUser).then(
            response => {
                this.setState({
                    error: "",
                    shareUser: "",
                    message: "File shared"
                });
                this.refreshFiles();
            },
            error => {
                this.setState({
                    error: "Someting went wrong",
                    message: ""
                });
            }
        );
    }
  
    render() {
        const {files,currentUser,message,error,shareForm} = this.state;
      return (
        <div className="container">
            {currentUser && 
            <h3>{currentUser.username}'s files</h3>}

            {message && <div className="alert alert-success" role="alert">{message}</div>}
            {error && <div className="alert alert-danger" role="alert">{error}</div>}
            <button className="btn btn-primary" type="button" data-toggle="collapse" data-target="#collapseUpload" aria-expanded="false" aria-controls="collapseUpload">
            Upload file
            </button>
            <div className="collapse" id="collapseUpload">
                <div className="card card-body">
                    <Form onSubmit={this.onFileUpload} ref={c => {this.form = c;}}>
                        <div className="form-group">
                        <Input type="file" className="form-control-file" name="file" onChange={this.onChangeFile} validations={[required]} />
                        </div>
                        <div className="form-group">
                        <button
                            className="btn btn-primary btn-block"
                            disabled={this.state.loading}
                        >
                            {this.state.loading && (
                                <span className="spinner-border spinner-border-sm"></span>
                            )}
                            <span>Upload</span>
                        </button>
                    </div>
                        <CheckButton
                        style={{ display: "none" }}
                        ref={c => {
                            this.checkBtn = c;
                        }}
                    />
                    </Form>
                </div>
            </div>
          <div className="list-group">
              {files && files.map((file,index) =>
              <li key={file.filename} className="list-group-item justify-content-between align-items-center">
                <div className="container">
                <div className="row">
                <div className="col-10">{file.filename}</div>
                <div className="col-2 btn-group">
                <button onClick={() => this.downloadFile(file.filename)} className="btn btn-primary">Download</button>
                <button type="button" className="btn btn-primary dropdown-toggle dropdown-toggle-split" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    <span className="sr-only">Toggle Dropdown</span>
                </button>
                <div className="dropdown-menu">
                    {/* <button className="dropdown-item" type="button" data-toggle="collapse" data-target={"#file" + index} aria-expanded="false" aria-controls={"file" + index}>Share</button> */}
                    <button className="dropdown-item" type="button" onClick={() => this.setShareForm(index)}>Share</button>
                    <div className="dropdown-divider"></div>
                    <button className="dropdown-item" onClick={(e) => { if (window.confirm('Are you sure you wish to delete this file?')) this.deleteFile(file.filename) }}
                    >Delete file</button>
                </div>
                </div>
                </div>
                </div>
                {/* <div className="collapse" id={"file"+index}> */}
                {shareForm === index && <div className="row">
                <div className="col-5 card card-body">
                    <Form onSubmit={this.onShareSubmit} ref={c => {this.form = c;}}>
                        <div className="form-group">
                        <label htmlFor="share">Share with</label>
                        <Input type="text" className="form-control" name="share" onChange={this.onChangeShare} validations={[required]} />
                        </div>
                        <input type="hidden" name="file" value={file.filename} 
                       ref={(input) => { this.fileShare = input }} />

                        <div className="form-group">
                        <button
                            className="btn btn-primary btn-block"
                            disabled={this.state.loading}
                        >
                            {this.state.loading && (
                                <span className="spinner-border spinner-border-sm"></span>
                            )}
                            <span>Share</span>
                        </button>
                    </div>
                        <CheckButton
                        style={{ display: "none" }}
                        ref={c => {
                            this.checkBtn = c;
                        }}
                    />
                    </Form>
            </div>
            <div className="col-5">
                Shared with:
                <ul>
                {file.sharedWith.map(user => <li key={user}>{user}</li>)}
                </ul>
            </div>
            </div>}
                {/* </div> */}
              </li>)}
            </div>
        </div>
      );
    }
  }
  