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
