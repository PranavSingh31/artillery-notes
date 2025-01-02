# Artillery

Modern open-source performance testing toolkit for SRE and DevOps. 

[GitHub Repository](https://github.com/artilleryio/artillery)  
Built on top of JavaScript.

---

## How to Download?

```bash
sudo npm install -g artillery@latest
artillery version
artillery dino -m "QAInsights" -r
```

---

## Test an API

Use a YAML configuration file:

```yaml
config:
  target: https://localhost:9966/petclinic/api
scenarios:
  - name: petclinic
    flows:
    - get:
        url: "/pets"
    - get:
        url: "/pettypes"
```

Run the test:

```bash
artillery run pets.yml
```

---

## Phases in Artillery

Defines Workload Model - basically how you want to inject the load.

- **X-axis**: Time (seconds)
- **Y-axis**: Virtual users (number)

Phases are defined using the `config.phases` section.

### Types of Phases

1. **Duration with constant arrival rate**
2. **Ramp-up with increasing arrival rate**
3. **Fixed count of arrival rate over time**
4. **Pause with no virtual users over time**

### Examples:

#### Pause Phase

```yaml
config:
  target: https://localhost:9966/petclinic/api
  phases:
    name: nothing
    pause: 10
```
No virtual users for 10 seconds.

#### Constant Arrival Rate

```yaml
config:
  target: https://localhost:9966/petclinic/api
  phases:
    name: constant_arrival_rate
    duration: 60
    arrivalRate: 10
```
Injects 10 virtual users every second for 60 seconds.

#### Ramp-up Arrival Rate

```yaml
config:
  target: https://localhost:9966/petclinic/api
  phases:
    name: rampup_rate
    duration: 60
    arrivalRate: 10
    rampTo: 50
```
Ramps up virtual users from 10 to 50 in 60 seconds.

#### Fixed Arrival Count

```yaml
config:
  target: https://localhost:9966/petclinic/api
  phases:
    name: fixed_Arrivals
    duration: 60
    arrivalCount: 120
```
Creates 120 virtual users in 1 minute.

---

## Payload

For data parameterization in Artillery. Use dynamic data to feed into requests.

### Configuration Example:

```yaml
config:
  target: https://localhost:9966/petclinic/api
  phases:
    ...
  payload:
    path: <test-data-path>
    order: sequence           # Default: random
    loadAll: true
    skipHeader: true          # Default: false
    delimiter: ","           # Default: ,
    skipEmptyLines: true      # Default: true
    fields:
      - "firstName"
      - "lastName"
      - "id"
      - "specialityName"

scenarios:
- name: petclinic_Add_vets 
  flow:
  - post:
      url: /vets
      headers:
        Content-Type: application/json
      json:
        firstName: "{{ firstName }}"
        lastName: "{{ lastName }}"
        id: "{{ id }}"
        specialities:
          id: "{{ id }}"
          name: "{{ specialityName }}"
```

---


## Correlation

Handling dynamic data in the response.

- Fetch the response using **regex**, store it into a variable, and pass it into subsequent requests.

### Example:

```yaml
config:
  target: https://example.com

scenarios:
  - name: example_dot_com
    flow:
      - get:
          url: "/"
          capture:
            - strict: true
              regexp: "<title>(.+?)<\\/title>"
              as: "extract_title"
              group: 1
              flags: g
            - strict: false
              header: "content-length"
              as: "extract_content_length"
              group: 1
      - log: "Extracted Title: {{ extract_title }}"
      - log: "Extracted Content Length: {{ extract_content_length }}"
```

## Debugging

these commands would execute the corresponding urls only

`DEBUG=http:request artillery run petstore.yml`

`DEBUG=http:response artillery run petstore.yml`

`DEBUG=http:request,http:response artillery run petstore.yml`

`DEBUG=http* artillery run petstore.yml`


## Expectations

- Assertions via Plugins
- use `npm install artillery-plugin-expect`
- `DEBUG=plugin:expect artillery run pets.yaml`

Expectation in artillery

- statusCode
- hasHeader
- contentType
- headerEquals
- hasProperty and notHasProperty
- matchesRegexp
- equals

```yaml
config:
  target: http://localhost:9966/petclinic/api
  plugins:
    expect: {}  #declation of plugin
  
  environments:
    load:  
      phases:
        - name: smoke_load_testing
          duration: 5
          arrivalRate: 10
      
    functional:
      phases:
        - name: func_smoke_testing
          duration: 1
          arrivalCount: 1

  payload:
    path: ./test-data/vets-data.csv
    order: sequence           # default: random
    loadAll: true             
    skipHeader: true          # default: false  
    delimiter: ","            # default: ,
    skipEmptyLines: true      # default: true
    fields:
      - "firstName"
      - "lastName"
      - "id"
      - "specialtyName"

scenarios:
- name: petclinic_add_vets
  flow:
  - post:
      url: /vets
      headers:
        Content-Type: application/json
      json:
        firstName: "{{ firstName }}"
        lastName: "{{ lastName }}"
        id: "{{ id }}"
        specialties:
          -
            id: "{{ id }}"
            name: "{{ specialtyName }}"
      capture:
        json: "$.id"
        as: "id"

      expect:				#expected items
        - matchesRegexp: .+ 
  - get:
      url: "/vets/{{ id }}"     
      expect:				#expected items
      - statusCode: 200
      - contentType: json
```
## Environments

- enables reusability
- same script for Function Testing and Performance Testing

```yaml

#run load or functional as per your requirement

environments:
  load:
    phases:
      - name: smoke_load_testing
        duration: 10
        arrivalRate: 10
  functional:
    phases:
      - name: smoke_func_testing
        duration: 1
        arrivalCount: 1

```

`artillery run --environment load vets_extraction.yml --output smoke.json

## HTML Reporting

- involves two-step process
- generate a JSON
- then, convert  it to HTML
  
	`artillery run --output loadtest-result.json pets.yaml`

        `artillery report loadtest-result.json`
